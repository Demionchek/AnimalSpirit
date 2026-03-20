namespace Features.Core.ObjectPool.Infrastructure
{
    public interface IObjectPool<T>
    {
        T Get();
        void Release(T obj);
    }
}