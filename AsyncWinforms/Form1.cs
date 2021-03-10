using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
 * So async is really like an invisible callback, every time you put await in your code it's like saying
 * Task.Run().then(... some code)
 * 
 * So 
 * Console.WriteLine("Starting a task");
 * await someTask;
 * Console.WriteLine("Task is done");
 * 
 * is like saying
 * 
 * Console.WriteLine("Starting a task");
 * someTask.Run(), then
 * {
 *      Console.WriteLine("Task is done.");
 * }
 */

namespace AsyncWinforms
{
    public partial class Form1 : Form
    {
        public void WriteLine(string str)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(WriteLine), str);
                return;
            }

            rtbText.AppendText($"{str}{Environment.NewLine}");
            
        }

        public Form1()
        {
            InitializeComponent();
        }

        private async Task<CToast> ToastBreadAsync(int slices)
        {
            for (int slice = 0; slice < slices; slice++)
            {
                WriteLine("Putting a slice of bread in the toaster");
            }

            WriteLine("Start toasting...");

            // to try and illustrate the concept I put in unnecessary curly braces delimiting where the callbacks would be

            // (return)
            await Task.Delay(3000); // then () =>
            {
                WriteLine("Remove toast from toaster");

                return new CToast(slices);
            }
        }

        private async Task<CBacon> FryBaconAsync(int slices)
        {
            WriteLine($"putting {slices} slices of bacon in the pan");
            WriteLine("cooking first side of bacon...");

            await Task.Delay(3000); // then () =>
            {
                for (int slice = 0; slice < slices; slice++)
                {
                    WriteLine("flipping a slice of bacon");
                }
                WriteLine("cooking the second side of bacon...");

                await Task.Delay(3000); // then () =>
                {
                    WriteLine("Put bacon on plate");

                    return new CBacon(slices);
                }
            }
        }

        async Task<Sandwich> MakeBLT()
        {
            Task<CToast> toastTask = ToastBreadAsync(2);
            Task<CBacon> baconTask = FryBaconAsync(3);

            await Task.WhenAll(toastTask, baconTask); // then (toast, bacon) =>
            {
                return new Sandwich(toastTask.Result, baconTask.Result);
            }
        }

        async Task PrintResult(Task<Sandwich> sandwichTask)
        {
            Sandwich sandwich = await sandwichTask; // then (sandwich) =>
            {
                WriteLine($"Your {sandwich.ToString()} is done!");
            }
        }

        void WrongWay()
        {
            // this will be a deadlock, there's actually only one thread. The async methods run on the same thread!
            Task<Sandwich> sandwichTask = MakeBLT();
            TaskAwaiter<Sandwich> awaiter = sandwichTask.GetAwaiter();
            Sandwich result = awaiter.GetResult();
            
       
        }

        private void btnRunAsync_Click(object sender, EventArgs e)
        {
            // the thing is, the first question on anyone's mind is how to run an async method from a non-async one.
            // otherwise async will infect your whole codebase.
            //
            // the blogs say that you should just let that happen but nobody told you what happens if you can't make the switch 100% right now

            Task<Sandwich> sandwichTask = MakeBLT();


            // The Javascripty way to then print the result would be something like
            // when done (sandwich) => { ... (the rest of the function)

            // I could do that actually
            Task printTask = PrintResult(sandwichTask);
            WriteLine("OK the button handler is all done now.");

            // we don't need to wait, and we shouldn't. We specified what gets ran when the task is done using PrintResult



            // though, there's some other weirder cases worth investigating, like in JS, you can do something like print(await GetResult())
            // and it will wait to call print until the result is there.
        }

    }

    public class CToast
    {
        public int Slices { get; }
        public CToast(int iSlices)
        {
            this.Slices = iSlices;
        }
    }

    public class CBacon
    {
        public int Slices { get; }

        public CBacon(int iSlices)
        {
            this.Slices = iSlices;
        }
    }

    public class Sandwich
    {
        public CToast Toast;
        public CBacon Bacon;
        public Sandwich(CToast iToast, CBacon iBacon)
        {
            this.Toast = iToast;
            this.Bacon = iBacon;
        }

        public override string ToString()
        {
            return $"Sandwich with {this.Toast.Slices} pieces of toast and {this.Bacon.Slices} pieces of bacon";
        }
    }
}
