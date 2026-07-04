# Idle Repair Tycoon — Unity Mobile

Protótipo de jogo **idle/tycoon mobile** com tema de assistência técnica. Esta versão já está na **Fase 2**, com uma interface mais visual e mais próxima de um joguinho de celular.

## Estado atual

- Projeto Unity em C#.
- Tela vertical pensada para mobile.
- Interface criada automaticamente por código.
- Oficina visual com fluxo de clientes, balcão, bancadas e caixa.
- Clientes animados andando pela oficina.
- Dinheiro subindo automaticamente.
- Bancadas/serviços desbloqueáveis.
- Upgrades por estação.
- Barras de progresso em cada serviço.
- Popups de dinheiro quando entra grana.
- Salvamento local com `PlayerPrefs`.
- Ganho offline ao voltar para o jogo.
- Turbo 2x por anúncio recompensado fake.
- Pacote de dinheiro por anúncio fake.
- Prestígio/recomeço com multiplicador permanente.

## Como abrir

1. Instale o Unity Hub.
2. Instale uma versão atual do Unity com Android Build Support.
3. Abra este repositório como projeto Unity.
4. Abra qualquer cena vazia.
5. Aperte Play.

O jogo se inicializa sozinho usando `RuntimeInitializeOnLoadMethod`, então não precisa montar a cena manualmente para testar.

## Configuração da aba Game

Na aba **Game** do Unity, use uma proporção vertical:

- Aspect: `9:16`; ou
- resolução customizada: `1080 x 1920`.

Se a escala estiver muito alta, tipo `4.4x`, reduza para `0.5x` ou `1x` para visualizar a tela inteira.

## Arquivos principais

- `Assets/IdleRepairTycoon/Scripts/IdleGameBootstrap.cs`: cria o jogo automaticamente.
- `Assets/IdleRepairTycoon/Scripts/IdleGameController.cs`: lógica principal, save, dinheiro, offline e prestígio.
- `Assets/IdleRepairTycoon/Scripts/IdleGameState.cs`: dados e fórmulas do jogo.
- `Assets/IdleRepairTycoon/Scripts/IdleGameUI.cs`: interface mobile visual da Fase 2.
- `Assets/IdleRepairTycoon/Scripts/IdleGameAds.cs`: anúncios fake e preparação para AdMob.

## Como gerar APK

1. Vá em `File > Build Profiles` ou `File > Build Settings`.
2. Selecione Android.
3. Clique em `Switch Platform`.
4. Em Player Settings, configure:
   - Orientation: Portrait;
   - Package Name: `com.tpoll.idlerepairtycoon` ou outro nome seu;
   - Target API: Highest Installed ou API exigida pela Play Store.
5. Build.

## AdMob

Por padrão, o jogo usa anúncios falsos para testar a mecânica sem depender da conta AdMob.

Locais já preparados para monetização:

- botão `Turbo 2x`;
- botão `Pacote R$`;
- tela de ganho offline com opção de dobrar recompensa.

Antes de publicar com anúncio real, integrar o Google Mobile Ads Unity Plugin e usar IDs de teste durante o desenvolvimento.

## Próxima fase sugerida

Fase 3:

- sprites próprios;
- técnicos/personagens desenhados;
- sons de dinheiro e upgrade;
- loja de upgrades em modal;
- missões diárias;
- tela inicial;
- ícone do app;
- AdMob real;
- build APK/AAB para teste.
