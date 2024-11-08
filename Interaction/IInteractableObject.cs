namespace ShroomJamGame.Interaction;

public interface IInteractableObject
{
    public void Interact();

    public string GetOnHoverText();
}