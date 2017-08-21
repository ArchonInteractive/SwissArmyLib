namespace Archon.SwissArmyLib.Pooling
{
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }
}