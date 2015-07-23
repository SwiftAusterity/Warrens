namespace NetMud.Websock
{
    internal class NotificationMessage
  {
    public string Body { get; set; }
    public string Icon { get; set; }

    public string Summary { get; set; }

    public override string ToString ()
    {
      return string.Format ("{0}: {1}", Summary, Body);
    }
  }
}
