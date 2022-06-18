using Unity.Entities;
using UnityEngine;

namespace Reese.Demo
{
    public class RotatorAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField]
        Vector3 fromRelativeAngles = new Vector3(0, 0, 0);

        [SerializeField]
        Vector3 toRelativeAngles = new Vector3(0, 0, 0);

        [SerializeField]
        float frequency = 1;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new Rotator
            {
                FromRelativeAngles = fromRelativeAngles + transform.localRotation.eulerAngles,
                ToRelativeAngles = toRelativeAngles + transform.localRotation.eulerAngles,
                Frequency = frequency
            });
        }
    }
}
