using GameFramework;
using UnityGameFramework.Runtime;

/// <summary>
/// Persistent framework settings for localization and generic sound groups.
/// </summary>
public static class SettingExtension
{
    public static void SetABTestGroup(this SettingComponent component, string groupName)
    {
        component.SetString(ConstBuiltin.Setting.ABTestGroup, groupName ?? string.Empty);
    }

    public static string GetABTestGroup(this SettingComponent component)
    {
        return component.GetString(ConstBuiltin.Setting.ABTestGroup, string.Empty);
    }

    public static void SetLanguage(this SettingComponent component, GameFramework.Localization.Language language, bool saveSetting = true)
    {
        GFBuiltin.Localization.Language = language;
        component.SetString(ConstBuiltin.Setting.Language, language.ToString());
    }

    public static GameFramework.Localization.Language GetLanguage(this SettingComponent component)
    {
        string value = component.GetString(ConstBuiltin.Setting.Language, string.Empty);
        return System.Enum.TryParse(value, out GameFramework.Localization.Language language)
            ? language
            : GameFramework.Localization.Language.Unspecified;
    }

    public static void SetMediaMute(this SettingComponent component, Const.SoundGroup group, bool isMuted)
    {
        component.SetMediaMute(group.ToString(), isMuted);
    }

    public static bool GetMediaMute(this SettingComponent component, Const.SoundGroup group, bool defaultValue = true)
    {
        return component.GetMediaMute(group.ToString(), defaultValue);
    }

    public static void SetMediaVolume(this SettingComponent component, Const.SoundGroup group, float volume)
    {
        component.SetMediaVolume(group.ToString(), volume);
    }

    public static float GetMediaVolume(this SettingComponent component, Const.SoundGroup group, float defaultValue = 1f)
    {
        return component.GetMediaVolume(group.ToString(), defaultValue);
    }

    /// <summary>
    /// 按**分组名**读写静音。名字版是枚举版的基础，也是数据表驱动的那条路
    /// （`SoundGroupTable` 的行是名字，新增分组不该逼着框架层改枚举）。
    /// </summary>
    public static void SetMediaMute(this SettingComponent component, string groupName, bool isMuted)
    {
        if (component == null || string.IsNullOrWhiteSpace(groupName))
        {
            return;
        }

        var soundGroup = GF.Sound.GetSoundGroup(groupName);
        if (soundGroup == null)
        {
            return;
        }

        soundGroup.Mute = isMuted;
        component.SetBool($"Sound.{groupName}.Mute", isMuted);
    }

    public static bool GetMediaMute(this SettingComponent component, string groupName, bool defaultValue = true)
    {
        return component == null || string.IsNullOrWhiteSpace(groupName)
            ? defaultValue
            : component.GetBool($"Sound.{groupName}.Mute", defaultValue);
    }

    /// <summary>
    /// 按**分组名**读写音量。注意这里写入的 `Sound.&lt;分组&gt;.Volume` 是**最终生效音量**
    /// （主音量已经乘进去）——这样启动时把每一行读回来就能直接恢复，框架层不需要知道主音量的存在。
    /// </summary>
    public static void SetMediaVolume(this SettingComponent component, string groupName, float volume)
    {
        if (component == null || string.IsNullOrWhiteSpace(groupName))
        {
            return;
        }

        var soundGroup = GF.Sound.GetSoundGroup(groupName);
        if (soundGroup == null)
        {
            return;
        }

        soundGroup.Volume = volume;
        component.SetFloat($"Sound.{groupName}.Volume", volume);
    }

    public static float GetMediaVolume(this SettingComponent component, string groupName, float defaultValue = 1f)
    {
        return component == null || string.IsNullOrWhiteSpace(groupName)
            ? defaultValue
            : component.GetFloat($"Sound.{groupName}.Volume", defaultValue);
    }
}
