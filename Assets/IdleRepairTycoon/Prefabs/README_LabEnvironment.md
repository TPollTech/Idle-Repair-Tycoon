# LabEnvironment.prefab

Esta pasta é para o ambiente editável do jogo.

## Objetivo

Criar aqui o prefab oficial do laboratório/assistência técnica:

`Assets/IdleRepairTycoon/Prefabs/LabEnvironment.prefab`

## Estrutura oficial criada

Monte um GameObject vazio chamado `LabEnvironment` e organize os objetos seguindo estas pastas/grupos:

```text
LabEnvironment
├── 00_Floor
├── 01_Walls
├── 02_FrontCounter
├── 03_Cashier
├── 04_Workbenches
│   ├── Workbench_01_Peliculas
│   ├── Workbench_02_Baterias
│   ├── Workbench_03_Telas
│   ├── Workbench_04_Notebooks
│   └── Workbench_05_Premium
├── 05_Shelves
├── 06_WaitingArea
├── 07_Props
├── 08_Lighting
└── 09_CameraMarkers
```

## Assets que combinam

Use apenas assets que tenham sentido para uma assistência/laboratório:

- mesas;
- bancadas;
- cadeiras;
- balcões;
- computadores;
- monitores;
- caixas de peças;
- prateleiras;
- armários;
- luminárias;
- sofá de espera, se couber.

Evite assets aleatórios de natureza, quartos, cozinha, cama, pedras, árvores e decoração que não combine com o tema.

## Próximo passo

Depois de montar e salvar o prefab, faça commit e push. O código do jogo poderá ser ajustado para carregar esse prefab como ambiente oficial.
