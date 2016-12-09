namespace DistributedAlgoritmsClassLibrary
{
    public interface ReliableBroadcast : Broadcast
    {
        void Connect(Process process);
    }
}
