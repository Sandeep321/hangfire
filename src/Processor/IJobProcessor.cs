using System.Threading.Tasks;

namespace WebApi.Processor
{
    /// <summary>
    /// Interfaces for handling the processing of the scheduler
    /// </summary>
    public  interface IJobProcessor
    {
        Task Invoke();
    }
}