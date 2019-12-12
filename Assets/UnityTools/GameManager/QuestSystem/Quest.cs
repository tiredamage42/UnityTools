// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class Quest : MonoBehaviour
// {
//     // Start is called before the first frame update
//     void Start()
//     {
        
//     }

//     // Update is called once per frame
//     void Update()
//     {
        
//     }
// }



/*


    spawn options {
        scene


        spawnpoint name

        or 

        manual position / rotation


        stick to nav mesh option
    }
    spawnpoint manager {


    }

    class spawnpoint {

        isArea

        name:
            parentname.parentname.name

        position

        rotation
            or
        size (bounds)

        [NonSerilized] occupied

        bool IsOccupied () {
            return !usesBounds && occupied
        }


        Minitransform CalculateTransform (bool sticktonavmesh, bool check overlap) {
            calculate random position if usig bounds
            else
                take position

            if stick to navmesh:
                ground and navmehs hit

            check for nonstatic overlap
                spherecast (spawn radius)
                if overlapped:
                    shift position
                    retrun calcTransformInternal (shifted position, stick to nav, check overlap)

                else 
                    return calculated position

            

        }

    }


    spawn prefab at:
        spawn point
        spawn point within range of position



        spawn point is occupied (scene) :
            if scene loaded:
                check physics around spawn point

            else:
                keep list of used spawn points

*/

/*

    public class SpawnObject {
        public string key;
        public PrefabChoiceOrReference prefab;
        public SpawnLocation spawnLocation;
    }

 */