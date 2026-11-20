# Cenários de aceitação

**Verificação executada 0.8 — 09/11/2026:** o [QA-REPORT](QA-REPORT.md) associa A01–A31 aos testes/ensaios reais e explicita limites, incluindo tecnologia assistiva e escala. Última suíte 47/47; revisão independente e QA visual concluídos. A aprovação documental não foi usada como prova de execução; declarações de “não executado” abaixo são históricas das etapas de especificação.

Planejamento atual 0.7: IMPLEMENTATION_PLAN-0.7 (documento histórico removido na limpeza B14) mapeia A01–A31/R01–R21 a nove etapas, incluindo falhas entre estado/evento, MERGE parcial, reenvio, recuperação e quatro combinações de idioma/tema. A1 escolhida em [G2-A1-007](../records/G2-A1-007.md). Cenários abaixo são contratos para testes futuros; nenhuma aprovação documental conta como teste executado.

PROPOSTOS como tradução verificável de regras aprovadas. Não executados: não existe aplicativo. Escolhas pendentes não viram resultados obrigatórios sem aprovação.

Atualização 0.6: base G0/UX G1 0.5 aprovadas em [registro](../records/G0-G1-APPROVED-006.md). Comportamentos de estados/ciclos/filtros/lote e preferências não são mais dúvidas de UX. Cenários continuam especificação de verificação futura, com desenho de testes/comandos em G3; contratos técnicos de G2 0.6 (documento histórico removido na limpeza B14) ainda propostos. Nenhum cenário funcional foi executado por aprovação documental.

| Cenário | Requisitos | Evidência futura |
|---|---|---|
| A01 — cinco mídias | R01/R12 | Uma obra de cada mídia acessível/adicionável, persistente após reinício |
| A02 — anime | R02 | Mesmo ID em diferentes visualizações; só filme/série; mangá segue mídia própria |
| A03 — identidade e entrada | R03/R07 | Sem login; identidade estável; estado compatível obrigatório; reincluir não duplica |
| A04 — renomeação | R08 | ID/semântica preservados; não criar/excluir estados |
| A05 — transição/reconsumo | R09 | Ida/volta e novo consumo com histórico preservado e estado atual único |
| A06 — datas | R10 | Ausentes usam inclusão, origem visível; informadas prevalecem; registro real separado |
| A07 — notas | R11 | Texto pessoal privado, sem virar dado global ou rating |
| A08 — listas | R06 | Livro/jogo juntos, obra em várias listas, remoção da lista não muda estado |
| A09 — tags | R04/R05/R13 | Master mantém grupos/aplicabilidade; associação incompatível rejeitada |
| A10 — preview/cancelar | R14/R15 | Preview sem gravação; cancelar preserva dados |
| A11 — ausência no MERGE | R14 | Atualizar campo não apaga campo ausente ou associação fora do arquivo |
| A12 — parcial/atômico | R15 | Válida gravada, inválida rejeitada sem deixar vínculos incompletos |
| A13 — CSV problemático | R14/R15 | ;, aspas, duplicatas e referências tratados conforme contrato aprovado |
| A14 — filtros | R16 | Contexto e AND/OR/NOT têm resultados conferíveis e saída de busca vazia |
| A15 — lote | R17 | Estado incompatível nunca gravado; listas em lote funcionam em seleção mista |
| A16 — arquivo | R18 | Oculta ativa, conserva estado/histórico/datas/nota/vínculos, restaura sem duplicata |
| A17 — escopo | R03/R11–R15 | Sem login, ratings, CSV pessoal, exportação ou comunidade no MVP |

## Fixture booleana candidata

PROPOSTO: modo de inclusão AND ou OR, depois conjunto de exclusões NOT. Fórmula candidata: contexto ∩ inclusão − exclusões. Sem inclusão, considerar todo o contexto; NOT remove obra com qualquer tag excluída. Não se aprovou construtor booleano arbitrário.

X tem terror+espacial; Y tem terror; Z tem espacial+humor.
AND(terror, espacial) → X. OR(terror, espacial) → X,Y,Z.
OR(terror, espacial) com NOT(humor) → X,Y.
A gramática final depende do G1/G2.

PROPOSTO para lote misto: separar por mídia ou impedir confirmação enquanto houver incompatíveis, mostrando quais são. Sucesso parcial de CSV não aprova automaticamente sucesso parcial de estados em lote; são escolhas distintas.

Após a decisão técnica, transformar cenários em testes de domínio, integração e jornadas pertinentes. Cobrir falhas entre estado/histórico, reimportação e restauração. Não gerar testes de um aplicativo inexistente nesta fase.

## Complemento candidato 0.2

Detalha a alternativa de lote anteriormente aberta; não altera aprovação dos cenários. Todos são verificações FUTURAS, sem execução nesta rodada.

| Cenário | Vínculo | Resultado observável proposto |
|---|---|---|
| A18 — estado e evento atômicos | A05/R07/R09 | Falha entre as gravações não deixa estado novo sem evento ou evento novo sem estado |
| A19 — novo consumo no mesmo estado | A05–A06/R09–R10 | Novo ciclo explícito, mesmo acompanhamento, datas anteriores preservadas, fallback na inclusão original |
| A20 — correção e retroatividade | A06/R10 | Corrigir data mantém anterior auditável; instante de registro não muda; data retroativa não redefine estado atual |
| A21 — lote integral por mídia | A15/R17 | Grupo incompatível bloqueia confirmação; após revisão, conflito/falha aborta toda a seleção; CSV continua parcial |
| A22 — reenvio | A03/A05/R07/R09 | Repetir operação incerta não duplica acompanhamento, evento ou ciclo |
| A23 — preview obsoleto | A10–A13/R14–R15 | Alteração após preview impede gravar resultado não revisado |
| A24 — interrupção do CSV | A12/R15 | Commits concluídos preservados, demais não processadas, relatório recuperável; repetir não duplica |
| A25 — recuperação operacional | A01/A16/R18 | Backup consistente restaurado em cópia preserva identidade, estado, eventos, datas, nota e listas |
| A26 — teclado e largura estreita | UX_PROPOSAL/Q08 | Fluxos J1/J2/J5/J8 sem armadilha de foco, rótulos e erros anunciáveis; reflow sem perder ações |

Para A16, verificar também que restaurar não altera inclusão nem cria evento de consumo. Para A14, contexto e busca ficam fora do modo AND/OR; NOT exclui qualquer correspondência; critérios inaplicáveis não permanecem ocultos. Para A09, editar aplicabilidade não remove vínculos existentes silenciosamente.

## Revisão UX 0.4 — associação aos artefatos

Propostas apresentadas em G1_REVIEW-0.4 (documento histórico removido na limpeza B14), sem execução dos cenários: histórico visualiza A05/A06/A19/A20; lote visualiza A15/A21; CSV visualiza A10/A11/A12 e referência rejeitada de A13. Nenhum mock comprova atomicidade real, persistência ou recuperação. Em A14, recorte de mídia integra contexto e permanece restritivo com OR. Em A12, linha válida “sem alteração” conta no preview sem produzir duplicata ou promessa de nova gravação.

Exemplo adicional proposto para A14: contexto Biblioteca, recorte Jogos, Qualquer(terror,espacial) deve retornar só jogos com uma dessas tags; livros/filmes com as mesmas tags ficam fora. O recorte está explicitamente fora do modo AND/OR na fórmula 0.4. Este resultado é esperado para verificação futura, não consulta executada.

## Complemento 0.5 — temas e idiomas

R19–R21 estão aprovados; os cenários abaixo são propostas verificáveis, não testes executados.

| Cenário | Requisitos | Evidência futura |
|---|---|---|
| A27 — temas e preferência | R19/R03 | Alternar claro/escuro sem perder edição; preferência após reinício; contraste/foco/erros/controles nativos conferidos nos dois modos |
| A28 — interface completa PT-BR/EN | R20 | Navegação, campos, estados padrão, ajuda, erros, acessibilidade, plurais e administração traduzidos; quatro combinações de tema/idioma |
| A29 — conteúdo e identidade | R07–R11/R20 | Troca de idioma preserva IDs, estado, eventos, ciclos, título, tag/grupo, listas, nota e rótulo personalizado; Lendo padrão vira Reading sem nova transição |
| A30 — idioma adicional | R21 | Incluir catálogo/registro de localidade de teste sem editar domínio/telas ou migrar obras; validar chaves, parâmetros, plurais e layout; fallback legível |
| A31 — formato e falhas de preferência | R10/R14–R15/R19–R21 | Data de calendário não muda de dia, origem informada/fallback preservada, CSV canônico tem mesmo resultado; falha de preferência reportada sem alegar persistência ou perder rascunho |

Coverage/fallback e formato de preferência são candidatos G2; mock escuro/English demonstra intenção visual, não comprova tradução integral ou contraste. Não há app, preferência real gravada, novo idioma de teste ou testes funcionais executados nesta rodada.
