

using System;

public class MessageBoxContent : IServiceMessage
{
    public MessageBoxContent(float showForSeconds, string header, string body)
    {
        ShowForSeconds = showForSeconds;
        Header = header;
        Body = body;
        type = MsgBoxType.NoButtons;
    }

    public MessageBoxContent(float showForSeconds, string header, string body, Action buttonAction, string buttonText) : this(showForSeconds, header, body)
    {
        ButtonRightAction = buttonAction;
        ButtonRightText = buttonText;
        type = MsgBoxType.OneButton;
    }
    public MessageBoxContent(float showForSeconds, string header, string body, Action buttonRightAction, string buttonRightText, Action buttonLeftAction, string buttonLeftText) : this(showForSeconds, header, body, buttonRightAction, buttonRightText)
    {
        ButtonLeftAction = buttonLeftAction;
        ButtonLeftText = buttonLeftText;
        type = MsgBoxType.TwoButtons;
    }

    public MsgBoxType type;
    public float ShowForSeconds { get; set; }
    public string Header { get; set; }
    public string Body { get; set; }
    public Action ButtonRightAction { get; set; }
    public string ButtonRightText { get; set; }
    public Action ButtonLeftAction { get; set; }
    public string ButtonLeftText { get; set; }

    public enum MsgBoxType
    {
        NoButtons,
        OneButton,
        TwoButtons
    }

}