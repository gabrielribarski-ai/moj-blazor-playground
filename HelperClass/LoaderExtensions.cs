namespace IzracunInvalidnostiBlazor.HelperClass
{
    public static class LoaderExtensions
    {
        public static async Task WithLoader(this Task task, LoaderService loader, string message)
        {
            try
            {
                loader.Show(message);
                await task;
            }
            finally
            {
                loader.Hide();
            }
        }

        public static async Task<T> WithLoader<T>(this Task<T> task, LoaderService loader, string message)
        {
            try
            {
                loader.Show(message);
                return await task;
            }
            finally
            {
                loader.Hide();
            }
        }
    }

}
