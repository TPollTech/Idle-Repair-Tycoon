# Como ativar AdMob neste projeto

O projeto vem com anúncios falsos para testar a mecânica sem depender de conta AdMob.

## Passo a passo

1. Crie sua conta no Google AdMob.
2. Crie um app Android no AdMob.
3. Crie um bloco de anúncio Rewarded.
4. Instale o Google Mobile Ads Unity Plugin no Unity.
5. No Unity, configure o App ID Android do seu app.
6. Em Player Settings > Other Settings > Scripting Define Symbols, adicione:

```text
USE_ADMOB
```

7. Abra `Assets/IdleRepairTycoon/Scripts/IdleGameAds.cs`.
8. Troque o ID de teste por seu ID real apenas na hora de publicar.

## Locais onde o jogo chama anúncio

- Botão `Turbo 2x`: ativa multiplicador por 5 minutos.
- Botão `Pacote R$`: dá dinheiro equivalente a 90 segundos de produção.
- Tela de ganho offline: permite dobrar o valor ganho enquanto estava fora.

## Importante

Durante desenvolvimento, use anúncios de teste. Não clique em anúncios reais em builds de teste.
