# Unity 商队行军与路线显示方案（无 LineRenderer / 无 NavMesh）

本实现提供一套可直接放入 Unity 项目的核心脚本，满足以下约束：

- 4096x4096 地图。
- 同屏最多 `300 * 18 = 5400` 支队伍。
- 地形障碍、玩家主城领地、中立建筑连线边界都可作为阻挡。
- 非盟友不能穿过他人领地/边界。
- 玩家迁城、主城升级导致占地变化后可重算路径。
- 路径显示采用 **Mesh 带状几何**，不使用 `LineRenderer`。
- 路径搜索使用 **A***，不使用 `NavMesh`。

## 脚本结构

- `Assets/Scripts/March/WorldGrid.cs`：地图阻挡层（静态障碍 + 动态阻挡）。
- `Assets/Scripts/March/PathService.cs`：A* 搜索。
- `Assets/Scripts/March/TerritorySystem.cs`：主城占地、同盟关系、中立建筑连线边界。
- `Assets/Scripts/March/MarchTeam.cs`：队伍状态。
- `Assets/Scripts/March/RouteMeshRenderer.cs`：路线 Mesh 生成。
- `Assets/Scripts/March/MarchManager.cs`：派队、重算路径、队伍推进、路线渲染绑定。

## 最小接入

1. 在场景创建空物体挂 `MarchManager`。
2. 准备两个 `RouteMeshRenderer` 预制体：
   - 我方/盟友路线材质（绿色半透明）。
   - 其他玩家路线材质（红色半透明）。
3. 通过服务端同步：障碍、主城位置等级、同盟关系、中立建筑占领状态。
4. 调用：

```csharp
var ok = marchManager.TryDispatchTeam(
    teamId: 1001001,
    ownerPlayerId: 7,
    isAlly: true,
    start: new Vector2Int(120, 300),
    target: new Vector2Int(980, 1300),
    speed: 3.5f);
```

主城迁移或升级后：

```csharp
marchManager.TerritorySystem.MoveOrResizeCity(playerId, newCityCenter, cityLevel);
marchManager.TryRepath(teamId, currentCell, newTargetCell);
```

中立建筑连墙：

```csharp
marchManager.TerritorySystem.LinkNeutralBuildings(buildingA, buildingB);
```

## 性能建议

- 分帧做路径请求（例如每帧最多处理 20~50 条）。
- 对远距离队伍降低路径可视化精度（路径抽稀）。
- 队伍图标与路径分层渲染，减少 overdraw。
- 若对 GC 特别敏感，可把 `Dictionary/List` 换成对象池 + Native 容器。

