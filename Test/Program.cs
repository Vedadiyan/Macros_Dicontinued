using System;
using Macros.Autowire;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var test = new Consumer(1);
            Console.WriteLine(test.TestInjection.Id);
            test = new Consumer(2);
            Console.WriteLine(test.TestInjection.Id);
            test = new Consumer(1);
            Console.WriteLine(test.TestInjection.Id);
            test = new Consumer(2);
            Console.WriteLine(test.TestInjection.Id);
            Manualwire.BindRelative<Consumer>(()=> new Consumer(1));                      
        }
    }
    public class Consumer
    {
        [Autowired]
        public TestInjection TestInjection { get; set; }
        public Consumer(int i)
        {
            this.EnableAutowiring(new Scope(i));
        
        }
        public Consumer() {}
    }
    [Bind(typeof(TestInjection), BindingMethod = BindingMethods.RELATIVE)]
    public class TestInjection : ITestInjection
    {
        public int Id { get; }
        public TestInjection()
        {
            Id = new Random().Next();
        }
    }
    public interface ITestInjection
    {
        int Id { get; }
    }
}
