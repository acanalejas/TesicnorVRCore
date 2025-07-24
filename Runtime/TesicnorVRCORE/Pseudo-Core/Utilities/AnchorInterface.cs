using UnityEngine;

public interface AnchorInterface
{
    public bool IsAnchored();

    public void AnchorIt(GameObject _anchor);

    public void ReleaseIt(GameObject _anchor);

    public void CheckDistance();
    
    public void EnableWarning();
    
    public void DisableWarning();

}
