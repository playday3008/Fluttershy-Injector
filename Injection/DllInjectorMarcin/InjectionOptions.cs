namespace DLLInjectionMarcin
{
    public class InjectionOptions
    {
        public bool WaitForThreadExit { get; set; }

        public static InjectionOptions Defaults
        {
            get
            {
                return new InjectionOptions();
            }
        }
    }
}
