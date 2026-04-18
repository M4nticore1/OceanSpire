using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CreaturesList", menuName = "GameContent/Creatures List")]
public class CreaturesList : ScriptableObject
{
    private static CreaturesList _instance;
    public static CreaturesList Instance
    {
        get
        {
            if (_instance == null) {
                _instance = Resources.Load<CreaturesList>("Lists/CreaturesList");
            }
            return _instance;
        }
    }

    [SerializeField] private Creature[] creatures = null;
    public Creature[] Creatures => creatures;

}