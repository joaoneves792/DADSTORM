namespace DistributedAlgoritmsClassLibrary
{
    public interface EpochConsensus<Value>
    {
        void Propose(Value value);
        void Abort();
    }
}
