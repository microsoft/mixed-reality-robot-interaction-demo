using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IServiceConsumer<T>
{
    /// <summary>
    /// Consumes the service item from any source
    /// </summary>
    /// <param name="item"></param>
    void ConsumeServiceItem(T item, string serviceName);


}
