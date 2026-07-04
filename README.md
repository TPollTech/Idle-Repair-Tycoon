# Idle Repair Tycoon — Base Unity Mobile

Protótipo completo de um jogo idle mobile no estilo “fábrica/tycoon”, com tema de assistência técnica.

## O que já tem

- Jogo vertical para celular
- Dinheiro subindo sozinho
- Bancadas/serviços desbloqueáveis
- Upgrade de cada bancada
- Cálculo de lucro por segundo
- Salvamento local automático
- Ganho offline ao voltar para o jogo
- Prestígio/recomeço com multiplicador permanente
- Botões preparados para anúncios recompensados
- Ad service fake para testar sem AdMob
- Estrutura para integrar Google Mobile Ads depois

## Como abrir

1. Instale o Unity Hub.
2. Instale uma versão atual do Unity com Android Build Support.
3. Crie um projeto novo em Unity 2D Mobile.
4. Copie a pasta `Assets/IdleRepairTycoon` deste ZIP para dentro do seu projeto Unity.
5. Abra qualquer cena vazia.
6. Aperte Play.

O jogo se inicializa sozinho usando `RuntimeInitializeOnLoadMethod`.

## Como gerar APK

1. File > Build Profiles ou Build Settings.
2. Selecione Android.
3. Switch Platform.
4. Player Settings:
   - Orientation: Portrait
   - Package Name: algo como `com.tpoll.idlerepairtycoon`
   - Minimum API: conforme seu público
   - Target API: Automatic Highest Installed ou API exigida pela Play Store
5. Build.

## AdMob

Por padrão o jogo usa anúncios falsos para teste. Para AdMob real:

1. Instale o Google Mobile Ads Unity Plugin.
2. Configure seu App ID Android no plugin.
3. Adicione o define `USE_ADMOB` em Player Settings > Scripting Define Symbols.
4. Troque os IDs de teste pelos seus IDs reais em `IdleGameAds.cs`.

Nunca teste clicando em anúncios reais. Use IDs de teste durante desenvolvimento.

## Arquivos principais

- `IdleGameBootstrap.cs`: cria o jogo automaticamente.
- `IdleGameController.cs`: lógica principal, save, dinheiro, offline e prestígio.
- `IdleGameState.cs`: dados e fórmulas do jogo.
- `IdleGameUI.cs`: interface mobile criada por código.
- `IdleGameAds.cs`: anúncios fake e preparação para AdMob.

## Próximas melhorias sugeridas

- Sons e música leve
- Sprites próprios
- Animação de clientes andando
- Loja premium
- Missões diárias
- Tutorial inicial
- Eventos especiais
- Balanceamento fino de economia
