namespace Library.Throttle
{
    public class ThrottleState
    {
        public int BroadcastsCountFromPreviousSecond { get; set; }
        public bool Paused { get; set; }
    }
}