const DRAG_TYPE = 'application/x-sayeh-treeitem';

/**
 * Wires HTML5 drag and drop on the rows of a SayehTreeView. Drop indicators are computed here so a
 * dragover never round-trips to .NET; .NET is only called once per entered item (dragenter) to get the
 * positions where the drop is allowed (CanDrop / ItemDroppedCommand.CanExecute), and when the item is dropped.
 */
export function init(tree, dotNetRef, options) {
    let opts = Object.assign({ checkDrop: false, expandDelay: 800 }, options);
    let dragged = null;     // the dragged fluent-tree-item
    let current = null;     // { item, position } currently showing a drop indicator
    let dragSequence = 0;   // ignores answers arriving after their drag ended
    let allowedCache = new Map(); // item id -> Set of allowed positions, or null while .NET is answering
    let expandTimer = null;
    let expandTarget = null;

    const ownItemOf = el => {
        const item = el?.closest?.('fluent-tree-item');
        return item && item.closest('fluent-tree-view') === tree ? item : null;
    };
    const rowOf = item => Array.from(item.children).find(c => c.classList.contains('treeitem-row')) ?? null;
    const hasChildItems = item => Array.from(item.children).some(c => c.tagName === 'FLUENT-TREE-ITEM');

    function cancelExpand() {
        clearTimeout(expandTimer);
        expandTimer = null;
        expandTarget = null;
    }

    function clearIndicator() {
        current?.item.removeAttribute('data-sayeh-drop');
        current = null;
        cancelExpand();
    }

    function endDrag() {
        clearIndicator();
        dragged?.removeAttribute('data-sayeh-dragging');
        dragged = null;
        dragSequence++;
    }

    function getPosition(item, row, clientY) {
        const region = item.shadowRoot?.querySelector('.positioning-region') ?? row;
        const rect = region.getBoundingClientRect();
        const y = clientY - rect.top;
        if (y < 0 || y > rect.height) return null;
        if (y < rect.height * 0.25) return 'Before';
        if (y > rect.height * 0.75) return 'After';
        return 'Inside';
    }

    // Asks .NET (once per item and drag) for every position the dragged item may be dropped at on `item`.
    function requestAllowedPositions(item) {
        if (!opts.checkDrop || allowedCache.has(item.id) || dragged.contains(item)) return;
        allowedCache.set(item.id, null);
        const sequence = dragSequence;
        dotNetRef.invokeMethodAsync('GetAllowedDropPositionsAsync', dragged.id, item.id)
            .then(positions => { if (sequence === dragSequence) allowedCache.set(item.id, new Set(positions ?? [])); })
            .catch(() => { if (sequence === dragSequence) allowedCache.set(item.id, new Set()); });
    }

    function isAllowed(item, position) {
        if (!opts.checkDrop) return true;
        requestAllowedPositions(item); // in case dragover reached the item without a dragenter
        // Unknown yet: refuse until .NET answers; dragover keeps firing so the indicator shows up right after.
        return allowedCache.get(item.id)?.has(position) ?? false;
    }

    function onDragEnter(e) {
        if (!dragged) return;
        const item = ownItemOf(e.target);
        if (item && item.id) requestAllowedPositions(item);
    }

    function onDragStart(e) {
        const row = e.target.closest?.('.treeitem-row[draggable="true"]');
        const item = row && ownItemOf(row);
        if (!item || !item.id) return;
        endDrag();
        dragged = item;
        allowedCache = new Map();
        e.dataTransfer.effectAllowed = 'move';
        e.dataTransfer.setData(DRAG_TYPE, item.id);
        e.dataTransfer.setData('text/plain', row.textContent.trim());
        // Deferred so the drag image is captured before the item is dimmed.
        requestAnimationFrame(() => dragged === item && item.setAttribute('data-sayeh-dragging', ''));
    }

    function onDragOver(e) {
        if (!dragged) return;
        const item = ownItemOf(e.target);
        const row = item && item.id ? rowOf(item) : null;
        const position = row ? getPosition(item, row, e.clientY) : null;
        if (!position || dragged.contains(item) || !isAllowed(item, position)) {
            clearIndicator();
            return;
        }

        e.preventDefault();
        e.dataTransfer.dropEffect = 'move';
        if (current?.item !== item || current.position !== position) {
            clearIndicator();
            item.setAttribute('data-sayeh-drop', position.toLowerCase());
            current = { item, position };
        }

        // Hovering the middle of a collapsed node expands it so the item can be dropped deeper.
        if (position === 'Inside' && !item.expanded && hasChildItems(item)) {
            if (expandTarget !== item) {
                cancelExpand();
                expandTarget = item;
                expandTimer = setTimeout(() => {
                    if (current?.item === item) item.expanded = true;
                }, opts.expandDelay);
            }
        } else {
            cancelExpand();
        }
    }

    function onDragLeave(e) {
        if (dragged && !tree.contains(e.relatedTarget)) clearIndicator();
    }

    function onDrop(e) {
        if (!dragged || !current) return;
        e.preventDefault();
        const sourceId = dragged.id;
        const { item, position } = current;
        endDrag();
        dotNetRef.invokeMethodAsync('DropItemAsync', sourceId, item.id, position).catch(() => { });
    }

    tree.addEventListener('dragstart', onDragStart);
    tree.addEventListener('dragenter', onDragEnter);
    tree.addEventListener('dragover', onDragOver);
    tree.addEventListener('dragleave', onDragLeave);
    tree.addEventListener('drop', onDrop);
    tree.addEventListener('dragend', endDrag);

    return {
        setOptions: options => { opts = Object.assign(opts, options); },
        stop: () => {
            endDrag();
            tree.removeEventListener('dragstart', onDragStart);
            tree.removeEventListener('dragenter', onDragEnter);
            tree.removeEventListener('dragover', onDragOver);
            tree.removeEventListener('dragleave', onDragLeave);
            tree.removeEventListener('drop', onDrop);
            tree.removeEventListener('dragend', endDrag);
        }
    };
}
