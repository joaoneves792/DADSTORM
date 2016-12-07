namespace DistributedAlgoritmsClassLibrary
{
    public interface UniformConsensus<Value>
    {
        void Propose(Value value);
    }
}
