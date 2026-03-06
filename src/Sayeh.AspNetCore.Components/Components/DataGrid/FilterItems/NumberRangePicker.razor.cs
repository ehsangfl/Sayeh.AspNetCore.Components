using Microsoft.AspNetCore.Components;

namespace Sayeh.AspNetCore.Components;

partial class NumberRangePicker
{

    #region Fields

    //private Nullable<double> fromValue { get; set; }

    //private Nullable<double> toValue { get; set; }

    #endregion


    #region Properties

    [Parameter]
	public Nullable<double> FromNumber { get; set; }

    [Parameter]
    public Nullable<double> ToNumber { get; set; }

    [Parameter]
    public EventCallback<(Nullable<double> FromNumber, Nullable<double> ToNumber)> RangeSelected { get; set; }

    #endregion

    private void DataEntered()
    {
        //if (fromValue.HasValue)
        //    FromNumber = fromValue.Value;
        //if (toValue.HasValue))
        //    ToNumber = toValue.Value;

        RangeSelected.InvokeAsync((FromNumber, ToNumber));
    }

    //protected override void OnParametersSet()
    //{
    //    if (FromNumber.HasValue)
    //        fromValue = FromNumber.Value;
    //    else
    //        fromValue = null;
    //    if (ToNumber.HasValue)
    //        toValue = ToNumber.Value;
    //    else
    //        toValue = null;
    //    base.OnParametersSet();
    //}

}
