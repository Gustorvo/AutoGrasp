using Pixelplacement.XRTools;
using System.Linq;
using UnityEngine;

namespace VRoom
{
    /// <summary>
    /// RoomMapepr geometry wrapper
    /// </summary>
    public class RoomMapperGeometryProvider : MonoBehaviour
    {
        private void Awake()
        {
            if (RoomMapper.Instance)
            {
                RoomMapper.Instance.OnRoomMapped -= GetRoomGeometry;
                RoomMapper.Instance.OnRoomMapped += GetRoomGeometry;
            }
        }

        private void OnDestroy()
        {
            if (RoomMapper.Instance)
            {
                RoomMapper.Instance.OnRoomMapped -= GetRoomGeometry;
            }
        }

        private void GetRoomGeometry()
        {
            RoomItems.Instance.AddItems(ItemType.Wall, RoomMapper.Instance.Walls.ToList());
            RoomItems.Instance.AddItem(ItemType.Floor, RoomMapper.Instance.Floor);
            RoomItems.Instance.AddItem(ItemType.Ceiling, RoomMapper.Instance.Ceiling);
        }
    }
}