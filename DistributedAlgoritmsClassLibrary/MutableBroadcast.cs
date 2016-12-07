namespace DistributedAlgoritmsClassLibrary
{
    public interface MutableBroadcast : BestEffortBroadcast
    {
        void Connect(Process process);
    }
}
