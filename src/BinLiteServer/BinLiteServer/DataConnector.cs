using System.Reflection;

namespace BinLiteServer
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DataConnector : Attribute
    {
        public static void Call(string name)
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.GetCustomAttribute<DataConnector>() is not null)
                {
                    try
                    {
                        type.GetMethod(name)?.Invoke(null, null);
                        Logger.Debug($"Called `{name}` on `{type.Name}`");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Failed to call method `" + name + "` on `" + type.Name + "`: " + ex);
                    }
                }
            }
        }
    }
}
