> [!todo]+ Repository
> git@github.com:lolesch/DungeonCrawler.git
> 
> > [!warning] Access
> > this is a private repository so you cannot gain access before been added as a collaborator by the owner https://github.com/lolesch

> [!todo]+ Unity Editor
> Copy paste this into your browser: `unityhub://2021.3.14f1/eee1884e7226`
> or use this link: https://unity.com/releases/editor/whats-new/2021.3.14

# Coding Convention

### Component References
1. If a class requires access to a component on the same GameObject level consider
```cs
[RequireComponent(typeof(SampleType))]
public class SampleClass { }
```
2. For public references use 
```cs
[SerializeField, ReadOnly] protected SampleType sample;
public SampleType Sample => sample != null ? sample : sample = GetComponent<SampleType>();
```
 and consider logging missing components
```cs
void OnEnable() {
	if (!Sample)
		UIExtensions.MissingComponent(nameof(SampleReference), gameObject);
}```
