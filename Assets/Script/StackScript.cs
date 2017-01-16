using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StackScript : MonoBehaviour {

    // Default-size for blocks
    private const float BOUNDS_SIZE = 3.5f;

    // Margin for the block-movement
    private const float BOUNDS_MARGIN = 1.0f;

    // Movement speed for the camera
    private const float STACK_SPEED = 5.0f;

    // Error-tolerance
    private const float ERROR_MARGIN = 0.1f;

    // Gain combo from this point on
    private const int COMBO_GAIN_STEP = 5;

    // Combo gains as much space every move
    private const float STACK_BOUNDS_GAIN = 0.25f;

    // Block vovement - current position, speed
    private float tileTransition = 0.0f;
    private float tileSpeed = 2.5f;

    // Position for new spawning tiles
    private float secondaryPosition;

    // Track-keeping of current combo-value
    private int combo = 0;

    // Array of all (12) blocks
    private GameObject[] theStack;

    // Default bounds for maximizing combo-gain to this
    private Vector2 stackBounds = new Vector2(BOUNDS_SIZE, BOUNDS_SIZE);

    // Game-score, increments per block spawned
    private int score = 0;

    // Identificator for the blocks
    private int stackIndex;

    // Disable movement at game over
    private bool gameOver = false;

    // Get info if moving on x- or z-axis
    private bool isMovingOnX = true;

    // Where the stack should be, for moving the stack down to keep it in the view of the camera
    private Vector3 desiredPosition;

    // Previous tile-position to calculate differences
    private Vector3 lastTilePosition;


    public Color32[] gameColors = new Color32[4];
    public Material stackMat;

    public Text scoreText;


    void Start()
    {
        theStack = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            theStack[i] = transform.GetChild(i).gameObject;
            ColorMesh(theStack[i].GetComponent<MeshFilter>().mesh);
        }
        // Begin count from the bottom
        stackIndex = transform.childCount - 1;
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (PlaceTile())
            {
                SpawnTile();
                score++;
                scoreText.text = score.ToString();
            } else
            {
                EndGame();
            }
        }

        MoveTile();

        // Move the stack for the camera to be in view
        transform.position = Vector3.Lerp(transform.position, desiredPosition, STACK_SPEED * Time.deltaTime);

    }

    private bool PlaceTile()
    {
        // false --> Lose game

        Transform t = theStack[stackIndex].transform;

        if (isMovingOnX)
        {
            // X - AXIS
            float deltaX = lastTilePosition.x - t.position.x;
            if(Mathf.Abs(deltaX) > ERROR_MARGIN)
            {
                // Cut the tile & reset combo
                combo = 0;
                stackBounds.x -= Mathf.Abs(deltaX);
                if (stackBounds.x <= 0)
                {
                    return false;
                }

                float middle = lastTilePosition.x + t.localPosition.x / 2;
                t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);

                // Generate falling rest of cutted block
                float rubblePosX;
                if(t.position.x > 0)
                {
                    rubblePosX = t.position.x + (t.localScale.x / 2);
                }
                else
                {
                    rubblePosX = t.position.x - (t.localScale.x / 2);
                }
                CreateRubble(
                    new Vector3(rubblePosX, t.position.y, t.position.z),
                    new Vector3(Mathf.Abs(deltaX),1,t.localScale.z)
                );
                t.localPosition = new Vector3(middle - (lastTilePosition.x / 2), score, lastTilePosition.z);

            }
            else
            {
                // Increase combo at perfect hit
                if (combo >= COMBO_GAIN_STEP && stackBounds.x < BOUNDS_SIZE)
                {
                    stackBounds.x += STACK_BOUNDS_GAIN;
                    float middle = lastTilePosition.x + t.localPosition.x / 2;
                    t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
                    t.localPosition = new Vector3(middle - (lastTilePosition.x / 2), score, lastTilePosition.z);
                }
                combo++;
                t.localPosition = new Vector3(lastTilePosition.x, score, lastTilePosition.z);
            }
        }
        else
        {
            // Z - AXIS
            float deltaZ = lastTilePosition.z - t.position.z;
            if (Mathf.Abs(deltaZ) > ERROR_MARGIN)
            {
                // Cut the tile
                combo = 0;
                stackBounds.y -= Mathf.Abs(deltaZ);
                if (stackBounds.y <= 0)
                {
                    return false;
                }

                float middle = lastTilePosition.z + t.localPosition.z / 2;
                t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);

                // Generate falling rest of cutted block
                float rubblePosZ;
                if (t.position.z > 0)
                {
                    rubblePosZ = t.position.z + (t.localScale.z / 2);
                }
                else
                {
                    rubblePosZ = t.position.z - (t.localScale.z / 2);
                }
                CreateRubble(
                    new Vector3(t.position.x, t.position.y, rubblePosZ),
                    new Vector3(t.localScale.x, 1, Mathf.Abs(deltaZ))
                );

                t.localPosition = new Vector3(lastTilePosition.x, score, middle - (lastTilePosition.z / 2));

            }
            else
            {
                if(combo >= COMBO_GAIN_STEP && stackBounds.y < BOUNDS_SIZE)
                {
                    stackBounds.y += STACK_BOUNDS_GAIN;
                    float middle = lastTilePosition.z + t.localPosition.z / 2;
                    t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
                    t.localPosition = new Vector3(lastTilePosition.x, score, middle - (lastTilePosition.z / 2));
                } 
                combo++;
                t.localPosition = new Vector3(lastTilePosition.x, score, lastTilePosition.z);
            }
        }


        if(isMovingOnX)
        {
            secondaryPosition = t.localPosition.x;
        } else
        {
            secondaryPosition = t.localPosition.z;
        }
        
        isMovingOnX = !isMovingOnX;
        return true;
    }

    // Spawn new tile from the bottom of the stack to the top
    private void SpawnTile()
    {
        lastTilePosition = theStack[stackIndex].transform.localPosition;
        stackIndex--;
        if (stackIndex < 0)
        {
            stackIndex = transform.childCount - 1;
        }

        desiredPosition = Vector3.down * score;
        theStack[stackIndex].transform.localPosition = new Vector3(0, score, 0);
        theStack[stackIndex].transform.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);

        ColorMesh(theStack[stackIndex].GetComponent<MeshFilter>().mesh);
    }

    // Set movement of tile (backwards + forwards)
    // for x and z axis
    private void MoveTile()
    {
        if (!gameOver)
        {
            tileTransition += Time.deltaTime * tileSpeed;
            if (isMovingOnX)
            {
                theStack[stackIndex].transform.localPosition = new Vector3(Mathf.Sin(tileTransition) * BOUNDS_SIZE + BOUNDS_MARGIN, score, secondaryPosition);
            }
            else
            {
                theStack[stackIndex].transform.localPosition = new Vector3(secondaryPosition, score, Mathf.Sin(tileTransition) * BOUNDS_SIZE + BOUNDS_MARGIN);

            }
        }
    }

    private void CreateRubble(Vector3 pos, Vector3 scale)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.AddComponent<Rigidbody>();

        go.GetComponent<MeshRenderer>().material = stackMat;
        ColorMesh(go.GetComponent<MeshFilter>().mesh);

    }

    // Lost game
    private void EndGame()
    {
        Debug.Log("Lose");
        gameOver = true;
        theStack[stackIndex].AddComponent<Rigidbody>();
    }

    private Color32 Lerp4(Color32 a, Color32 b, Color32 c, Color32 d, float t)
    {
        if (t < 0.33f)
            return Color.Lerp(a, b, t / 0.33f);
        else if (t < 0.66)
            return Color.Lerp(b, c, (t - 0.33f) / 0.33f);
        else
            return Color.Lerp(c, d, (t - 0.66f) / 0.66f);
    }

    private void ColorMesh(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Color32[] colors = new Color32[vertices.Length];

        float f = Mathf.Sin(score * 0.25f);

        for(int i = 0; i<vertices.Length; i++)
        {
            colors[i] = Lerp4(gameColors[0], gameColors[1], gameColors[2], gameColors[3], f);
        }
        mesh.colors32 = colors;
    }
}
