using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField]
    private SceneConnection _connection;

    [SerializeField]
    private string _targetSceneName;

    [SerializeField]
    private Transform _spawnPoint;

    private Animator transition;

    private void Start()
    {
        transition = GameObject.FindGameObjectWithTag("Fade").GetComponent<Animator>();

        if (_connection == SceneConnection.ActiveConnection)
        {
            FindAnyObjectByType<Player>().transform.position = _spawnPoint.position;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Player>();
        if (player != null)
        {
            SceneConnection.ActiveConnection = _connection;
            StartCoroutine(LoadNextScene());
        }
    }

    IEnumerator LoadNextScene()
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(0.35f);

        SceneManager.LoadScene(_targetSceneName);
    }

}
