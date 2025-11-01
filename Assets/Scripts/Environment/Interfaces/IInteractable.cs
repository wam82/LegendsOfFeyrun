namespace Environment.Interfaces
{
    public interface IInteractable
    {
        bool CanInteract { get; set; }
        void Interact();
    }
}