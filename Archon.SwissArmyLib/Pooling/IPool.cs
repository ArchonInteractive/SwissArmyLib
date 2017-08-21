namespace Archon.SwissArmyLib.Pooling
{
    public interface IPool<T>
    {
        T Spawn();
        void Despawn(T target);
    }
}
