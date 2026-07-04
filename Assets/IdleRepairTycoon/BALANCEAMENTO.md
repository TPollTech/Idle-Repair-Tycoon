# Balanceamento inicial

Este jogo usa um balanceamento simples para protótipo.

## Fórmulas

Lucro por serviço:

```text
lucro_base * 1.18^(nível - 1) * multiplicador_prestígio * multiplicador_turbo
```

Tempo por serviço:

```text
tempo_base / (1 + (nível - 1) * 0.035)
```

Custo de upgrade:

```text
lucro_base * 6.5 * 1.22^(nível - 1)
```

Ganho offline:

```text
lucro_por_segundo * segundos_fora * 0.55
```

Limite offline: 8 horas.

## Serviços iniciais

1. Películas
2. Baterias
3. Telas
4. Notebooks
5. Laboratório Premium

## Prestígio

Ao juntar R$ 250.000, o jogador pode reiniciar a loja e ganhar +12% de lucro permanente.
