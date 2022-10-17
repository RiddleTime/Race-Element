namespace ACC_Manager.Broadcast.Structs
{
    public struct ConnectionState
    {
        public int ConnectionId { get; set; }
        public bool ConnectionSuccess { get; set; }
        public bool IsReadonly { get; set; }
        public string Error { get; set; }
    }
}
