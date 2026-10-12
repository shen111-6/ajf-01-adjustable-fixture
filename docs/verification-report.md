# 软件验证报告 · v1.0.0

|检查|结果|证据|
|---|---|---|
|主装配重开|29实例、14种模型、28个有效锁定配合无错误|native-validation.txt|
|三个静态开口|25、60、100mm对应实际装配；边界工件25×20×10 /100×80×20|range-validation.txt及两张截图|
|定位与空间|A/B/C名义接触；最小挡块间隙5mm、底部螺栓间隙4mm；槽内中心75～150mm|component-transforms.txt、图纸坐标及range-validation.txt|
|旋钮连接|末端12mm啮合，修订了原先只有端面接触的错误|solid-interference.tsv：H06/P06项|
|实体干涉|每套11处螺纹包络重叠；未发现其他实体穿透|solid-interference-summary.txt、range-validation.txt|
|AutoCAD|六张DWG重开，22个原生尺寸对象，毫米单位|autocad-validation.txt|
|关联图纸|六张SLDDRW，每张四个关联模型视图，引用存在|native-validation.txt|
|可搬移引用|三个装配的全部29个部件解析自交付目录|portability-pack.txt、portability-reopen.txt|
|理论计算|400N假设下局部应力12MPa、挠度约0.00311mm；保留低摩擦裕量1.0限制|calculations.json/csv|

证据文件在[evidence](../evidence)目录。模型中的螺纹、倒角、内六角有简化；默认锁定装配不是运动仿真。未经加工和实物试验，不报告实测定位精度或已验证承载能力。

GitHub CI仅核对文件散列、数量、公式、链接和已记录的CAD证据一致性，不重新运行SolidWorks或实物测试。
