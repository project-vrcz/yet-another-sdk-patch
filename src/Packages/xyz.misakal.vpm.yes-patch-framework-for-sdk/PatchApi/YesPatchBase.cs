using JetBrains.Annotations;
using UnityEngine.UIElements;

namespace YesPatchFrameworkForVRChatSdk.PatchApi;

[PublicAPI]
public abstract class YesPatchBase
{
    public abstract string Id { get; }
    public abstract string DisplayName { get; }
    public virtual string Category { get; } = "Uncategorized";
    public virtual string Description { get; } = "No description provided.";

    public virtual bool IsDefaultEnabled { get; } = true;

    public abstract void Patch();
    public abstract void UnPatch();

    public virtual bool HasSettingsUi { get; }

    public virtual void OnSettingsUi()
    {
    }

    public virtual void CreateSettingsUi(VisualElement rootVisualElement)
    {
    }
}