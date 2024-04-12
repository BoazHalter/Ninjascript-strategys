public partial class TradingSpecifications
{
    public void Entry(string strategyName)
    {
        string instrumentFullName = Instrument.FullName.ToString();

        switch (instrumentFullName)
        {
                case instrumentFullName.Contains("MES"):
                Log("Submitting Entry orders for: " + instrumentFullName);

                // MES entry orders
                Log("Submit Long 1a share at: " + (GetCurrentBid(0) - TickSize * 2).ToString());
                EnterLongLimit(2, GetCurrentBid(0) - TickSize * 2, "Long 1a");

                Log("Submit Long 1b share at: " + (GetCurrentBid(0) - TickSize * 6).ToString());
                EnterLongLimit(1, GetCurrentBid(0) - TickSize * 6, "Long 1b");

                Log("Submit Long 1c share at: " + (Low[1] - TickSize * 13).ToString());
                EnterLongLimit(1, Low[1] - TickSize * 14, "Long 1c");
                break;
            case instrumentFullName.Contains("MNQ"):
                Log("Submitting Entry orders for: " + instrumentFullName);

                // MNQ entry orders
                Log("Submit Long 1a share at: " + (Low[1] - TickSize * 9).ToString());
                EnterLongLimit(2, Low[1] - TickSize * 9, "Long 1a");

                Log("Submit Long 1b share at: " + (Low[1] - TickSize * 13).ToString());
                EnterLongLimit(1, Low[1] - TickSize * 13, "Long 1b");

                Log("Submit Long 1c share at: " + (Low[1] - TickSize * 18).ToString());
                EnterLongLimit(1, Low[1] - TickSize * 18, "Long 1c");
                break;

            default:
                Log("Unsupported instrument: " + instrumentFullName);
                break;
        }
    }
   
    // Additional instrument specifications
    public int Size { get; set; }         // Number of shares
    public int StopLoss { get; set; }      // Stop-loss in ticks or other units
    public int Profit { get; set; }        // Profit target (partial, runner, etc.)
}
