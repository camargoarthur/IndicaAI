# Evidência da validação 0.10

Implementação autorizada em 12/11/2026, abrangendo B11 e a rotina obrigatória de manutenção dos testes.

## Resultados reais

| Verificação | Resultado |
|---|---|
| Baseline anterior | 47/47 xUnit, zero falhas, cerca de 8 segundos. |
| Produto após ampliação | **61/61**, zero falhas/omissões, aproximadamente 6,7 segundos no Full final. |
| Contratos Node | **23/23**, seleção por impacto, execução/falhas/omissões, guia e referências TM. |
| Modelo JavaScript | Passou: rascunhos, campos apagados, seleção/desmarcação, conflitos e exclusão de campos sensíveis. |
| Full | **8/8 etapas**: contratos, JS, build, produto, publish, pacote, iniciador, documentos. |
| Module / Preferences | **30/30 testes de produto**, cinco etapas aprovadas; evidencia filtro real por classes. |
| Pacote isolado | 22 recursos conferidos, executável/idiomas presentes, sem banco incluído. |
| Iniciador Windows PowerShell 5.1 pelo BAT | HTTP 200; host encerrado abruptamente → processo filho saiu e HTTP parou; reabertura dos mesmos dados e novo encerramento sem órfão. |

Evidência estruturada: [Full](VALIDATION-0.10-full.json), [Module](VALIDATION-0.10-module.json), [log operacional](VALIDATION-0.10-launcher.log). Logs completos estão nas pastas `.tools/validation/2026-11-12T22-12-32-925Z-21032` e `.tools/validation/2026-11-12T22-12-52-775Z-11684`. A checagem documental final após o fechamento fica em uma execução Docs separada; ela não exige repetir o produto inalterado.

## Falhas encontradas e verificadas

- Primeiro Full detectou `Path.GetRelativePath` indisponível no Windows PowerShell 5.1. Corrigido usando o caminho descendente enumerado; nova chamada do mesmo verificador passou.
- Segundo Full detectou módulos PowerShell Core herdados pelo Node impedindo carregar `Get-FileHash` no host 5.1. Ambiente dos scripts passou a usar módulos nativos do Windows; regressão executa comandos reais nesse host com caminho herdado incompatível.
- Ensaio CIM foi bloqueado no ambiente restrito. Adicionada checagem antes de iniciar processos; execução Full com permissão ampliada, após revisão automática, passou. Isso não é falha da aplicação. Em ambientes restritos, registrar impedimento em vez de declarar Full aprovado.
- Revisão independente somente leitura encontrou teste Node `skip` retornando código 0. Corrigido conferindo relatório TAP e rejeitando testes omitidos, pendentes, cancelados, vazios ou falhos. Teste de regressão RED→GREEN com processo Node real.
- Revisão também apontou logs fixos do iniciador. Achado reclassificado como importante para rastreabilidade prometida: agora cada rodada tem pasta exclusiva, incluindo stdout/stderr da primeira abertura e da reabertura. Caminhos reais constam no log operacional.

Testes retrospectivos validam comportamento já existente; não foram apresentados como RED de funcionalidades ausentes. Seletor/executor têm testes RED anteriores à implementação, incluindo saída de falha, timeout e zero testes. Não houve mudança nas regras do produto para fazer testes passarem.

## Processos, dados e limites

Servidor operacional de teste encerrado. Raiz usada: `.local/launcher test-29f55a90cb6549fbba59c7d583e4e76c`; porta 5276. Demais testes usam raízes temporárias GUID. Coleção pessoal, `.local/qa` e `.local/teste-manual-01` preservadas; nenhum servidor deixado ativo pela entrega.

Há avisos conhecidos de TestServer sobre limite de corpo HTTP e chaves locais sem criptografia. Eles não são apresentados como erros nem como ausência de avisos. Não houve automação integral de navegador, clique real no X ou aprovação automática dos 39 casos manuais. Mapa de cobertura e limites: [VALIDATION](../VALIDATION.md), [manual-coverage.json](../../tests/manual-coverage.json).
