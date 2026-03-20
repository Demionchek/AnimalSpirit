namespace Features.AI.Application.States
{
    public static class EnemyStateExtensions
    {
        public static T Init<T>(
            this T state,
            EnemyStateContext ctx)
            where T : EnemyState
        {
            state.Initialize(ctx);
            return state;
        }
    }
}