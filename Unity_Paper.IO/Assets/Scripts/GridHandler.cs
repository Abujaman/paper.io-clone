using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridHandler : MonoBehaviour
{
    public Quad quad; //single segment
    AudioSource audioSource;
    public AudioClip clip;
    public uint playerCount;
    private Camera cam;
    [SerializeField]
    private int xCount, yCount;
    public int padding;
    [SerializeField]
    private float stepTime = 0.2f;
    //private Quad currentQuad;
    private Vector2[] currentDirection = new Vector2[3];
    private Vector2[] currentPosition = new Vector2[3];

    private List<Vector2> allPositions = new List<Vector2>();
    private Dictionary<Vector2, Quad> dictionary = new Dictionary<Vector2, Quad>();

    List<Quad.Player> enumList = new List<Quad.Player> { Quad.Player.Player1, Quad.Player.Player2, Quad.Player.Player3 };// new List<Quad.Player>();

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        calculationPermit = false;
        BuildGrid((float)xCount, (float)yCount);

        for (int i = 0; i < playerCount; i++)
        {
            SetInitialPosition(enumList[i]);
        }
    }

    private void BuildGrid(float _xCount, float _yCount)
    {
        //align camera//
        float xPos = (_xCount - 1) / 2;
        float yPos = (_yCount - 1) / 2;
        cam.transform.position = new Vector3(xPos, yPos, cam.transform.position.z);

        float screenRatio = (float)Screen.width / (float)Screen.height;
        bool theGreater = (_xCount / _yCount > screenRatio) ? false : true;
        float size = (theGreater) ? _yCount / 2f : _xCount / (2f * screenRatio);
        cam.orthographicSize = size;

        //instantiate grid//
        for (int i = 0; i < _xCount; i++)
        {
            for (int j = 0; j < _yCount; j++)
            {
                Quad clone = (Quad)Instantiate(quad, new Vector2(i, j), transform.rotation);
                clone.SetStatus(Quad.Status.None, Quad.Player.None);

                dictionary.Add(new Vector2(i, j), clone);
                allPositions.Add(new Vector2(i, j));
            }
        }
    }

    private void SetInitialPosition(Quad.Player player)
    {
        if (padding >= Mathf.Min((float)xCount / 2, (float)yCount / 2))
            Debug.LogWarning("Choose a smaller border value");

        Vector2 randomPosition = new Vector2(Random.Range(padding, xCount - padding), Random.Range(padding, yCount - padding));

        PlaceQuad(randomPosition, Quad.Status.Owned, player); //center
        for (int i = 0; i < 4; i++)
        {
            PlaceQuad(Surroundings(randomPosition)[i], Quad.Status.Owned, player);
            PlaceQuad(SurroundingsDiagonal(randomPosition)[i], Quad.Status.Owned, player);
        }
        currentPosition[(int)player - 1] = randomPosition;
    }

    private void PlaceQuad(Vector2 position, Quad.Status status, Quad.Player player)
    {
        Quad current = dictionary[position];
        current.SetStatus(status, player);
    }

    private Vector2[] Surroundings(Vector2 centerPosition)
    {
        Vector2[] array = new Vector2[4];
        array[0] = centerPosition + Vector2.up;
        array[1] = centerPosition + Vector2.right;
        array[2] = centerPosition + Vector2.down;
        array[3] = centerPosition + Vector2.left;
        return array;
    }

    private Vector2[] SurroundingsDiagonal(Vector2 centerPosition)
    {
        Vector2[] array = new Vector2[4];
        array[0] = centerPosition + Vector2.up + Vector2.right;
        array[1] = centerPosition + Vector2.right + Vector2.down;
        array[2] = centerPosition + Vector2.down + Vector2.left;
        array[3] = centerPosition + Vector2.left + Vector2.up;
        return array;
    }

    private void Step(Vector2 _direction, Quad.Player player)
    {
        Vector2 current = currentPosition[(int)player - 1];
        currentPosition[(int)player - 1] += _direction;

        Quad toSwitch = dictionary[currentPosition[(int)player - 1]];

        switch (toSwitch.enumStatus)
        {
            case Quad.Status.None:
                {
                    calculationPermit = true;
                    PlaceQuad(currentPosition[(int)player - 1], Quad.Status.Claimed, player);
                    break;
                }
            case Quad.Status.Owned:
                {
                    if (toSwitch.enumPlayer == player) //own 
                    {
                        audioSource.PlayOneShot(clip);
                        calculationPermit = false;

                        FloodFill(player);
                        //FloodFill(Vector2.zero);
                        //FloodFillOnPlayer(currentPosition[(int)player - 1], player);
                        //Uncheck();
                    }
                    else 
                    {
                        PlaceQuad(currentPosition[(int)player - 1], Quad.Status.Claimed, player);
                    }
                    break;
                }
            case Quad.Status.Claimed:
                {
                    //calculationPermit = true;
                    if (toSwitch.enumPlayer == player) //own quad
                    {
                        Debug.Log("OwnDead");
                        //Time.timeScale = 0;
                    }
                    else
                    {
                        Debug.Log("OtherDead");
                        //Time.timeScale = 0;
                    }
                    break;
                }
        }
        dictionary[currentPosition[(int)player - 1]].SetToRed(stepTime);
        //try
        //{
        //    
        //}
        //catch (KeyNotFoundException)
        //{
        //    Debug.Log("Dead.");
        //}
    }



    private void FloodFill(Quad.Player player)
    {
        List<Vector2> newList = new List<Vector2>(allPositions);
        Fill(Vector2.zero, player);

        foreach (Vector2 pos in allPositions)
        {
            dictionary[pos].SetStatus(Quad.Status.Owned, player);
        }

        allPositions = newList;
    }



    private void Fill(Vector2 position, Quad.Player player)
    {
        foreach (Vector2 pos in Surroundings(position))
        {
            Quad q;
            if (dictionary.TryGetValue(pos, out q))
            {
                if (q.enumPlayer != player && allPositions.Contains(pos))
                {
                    allPositions.Remove(pos);
                    Fill(pos, player);
                }
            }
        }
    }



    private bool calculationPermit;
    IEnumerator Steps()
    {
        while (true)
        {
            yield return new WaitForSeconds(stepTime);
            Step(currentDirection[0], Quad.Player.Player1);
            Step(currentDirection[1], Quad.Player.Player2);  
        }
    }

 

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(Steps());
        }
        if (Input.GetKeyDown(KeyCode.W))
            currentDirection[0] = Vector2.up;
        if (Input.GetKeyDown(KeyCode.D))
            currentDirection[0] = Vector2.right;
        if (Input.GetKeyDown(KeyCode.S))
            currentDirection[0] = Vector2.down;
        if (Input.GetKeyDown(KeyCode.A))
            currentDirection[0] = Vector2.left;

        if (Input.GetKeyDown(KeyCode.UpArrow))
            currentDirection[1] = Vector2.up;
        if (Input.GetKeyDown(KeyCode.RightArrow))
            currentDirection[1] = Vector2.right;
        if (Input.GetKeyDown(KeyCode.DownArrow))
            currentDirection[1] = Vector2.down;
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            currentDirection[1] = Vector2.left;

        if (Input.GetKeyDown(KeyCode.N))
        {
            //FloodFill(Vector2.zero);
            //FillRemaining();
            // FloodFillOnPlayer();
        }
    }
    
   
}