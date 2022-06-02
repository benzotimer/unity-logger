using System.Collections;
using System.Text;
using UnityEngine;

public class LogSender : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(SendRandomLogsRoutine());
    }

    private IEnumerator SendRandomLogsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f);
            int random = Random.Range(0, 3);

            switch (random)
            {
                case 0:
                    Debug.Log(GetLongMessage(4000));
                    break;
                case 1:
                    Debug.LogWarning("Warning log");
                    break;
                case 2:
                    Debug.LogError("Error log");
                    break;
                default:
                    break;
            }
        }
    }

    private string GetLongMessage(int count)
    {
        StringBuilder str = new StringBuilder();

        for (int i = 0; i < count; i++)
        {
            str.Append("A");
        }
        
        return str.ToString();
    }
}