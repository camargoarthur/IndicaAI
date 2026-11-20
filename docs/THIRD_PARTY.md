# Origens, licenças e recursos locais

Inventário de bibliotecas efetivamente presentes no `.deps.json` do pacote autocontido: [THIRD_PARTY-PACKAGES.json](evidence/THIRD_PARTY-PACKAGES.json). Contém versões, autores, expressão de licença, repositório e hash SHA-512 informado pelo restore. Dependências de desenvolvimento permanecem nos lock files dos projetos, sem alegar que foram todas redistribuídas no executável.

Conferência final: o arquivo da licença SmartFormat veio em Windows-1252 (símbolo © no copyright). Foi recodificado para UTF-8 preservando exatamente o texto; cópias no pacote sincronizadas. Isso corrige codificação, sem alterar os termos da licença.

| Recurso | Origem / licença | Registro |
|---|---|---|
| SDK isolado 10.0.403 e runtime .NET/ASP.NET 10.0.14 | Distribuição oficial Microsoft; MIT e avisos de terceiros incluídos | Metadado e SHA-512 verificado em `.tools/logs/sdk-origin.json`; [licença](licenses/DOTNET-LICENSE.txt), [avisos](licenses/DOTNET-ThirdPartyNotices.txt) |
| EF Core / Microsoft.Data.Sqlite / Extensions | Pacotes oficiais NuGet, MIT | Versões 10.0.14, lock files e inventário; licença Microsoft acima |
| CsvHelper 34.0.0 | Josh Close; MS-PL **ou** Apache-2.0, usada a alternativa Apache-2.0 | [licença original](licenses/CsvHelper-LICENSE.txt), commit `a314ac89d1b528b13fbd15778372e4675aa5398f` |
| SmartFormat e extensões 3.6.2 | Projeto SmartFormat/axuno, MIT | [licença](licenses/SmartFormat-LICENSE.md), commit `d8d49b454b63a8d621c258f1b2f583824688d049` |
| SQLitePCLRaw 2.1.13 / SQLite | Eric Sink, Apache-2.0; biblioteca SQLite incluída conforme seus avisos | [licença](licenses/SQLitePCL.raw-LICENSE.TXT), inventário NuGet e avisos .NET |
| Newtonsoft.Json 13.0.4 / ZString 2.6.0 | James Newton-King / Cysharp; MIT | Textos em `licenses`, copiados do pacote oficial ou repositório do titular |
| Full case folding Unicode 17.0.0 | [Unicode UCD](https://www.unicode.org/Public/17.0.0/ucd/CaseFolding.txt), licença Unicode | Arquivos `Text/CaseFolding-17.0.0.txt` e `Text/UNICODE-LICENSE.txt`; tabela C+F, sem T/S |
| Libre Baskerville variável | [Google Fonts](https://github.com/google/fonts/tree/main/ofl/librebaskerville), SIL OFL 1.1 | TTF e OFL em `wwwroot/fonts`; família editorial de títulos |
| Source Sans 3 variável | [Google Fonts](https://github.com/google/fonts/tree/main/ofl/sourcesans3), SIL OFL 1.1 | TTF e OFL em `wwwroot/fonts`; interface |
| Ícones search/plus | [Tabler Icons](https://github.com/tabler/tabler-icons), MIT | SVGs originais e LICENSE em `wwwroot/icons`; sem SVG artesanal |
| Marca TMDB | [Arquivo de atribuição oficial](https://www.themoviedb.org/assets/v4/logos/v2/blue_square_2-d537fb228cf3ded904ef09b136fe3fec72548ebc1fea3fbbd1ad9e36364db38b.svg) | SVG original em `wwwroot/icons/tmdb.svg`; uso no crédito da busca externa conforme [orientação do TMDB](https://developer.themoviedb.org/docs/faq) |

As cinco capas de `wwwroot/covers` foram geradas com a ferramenta Image Gen na entrega do catálogo demonstrativo, como imagens individuais, a partir da direção 2 aprovada. São obras e nomes fictícios. Os pôsteres exibidos na prévia do TMDB a partir da 0.11 são consultados ao abrir a tela e ficam fora do pacote local.

| Arquivo | Conteúdo / uso |
|---|---|
| `alem-do-vale.png` | Jogo; viajante, vale e ruínas em azul/verde |
| `cidade-distante.png` | Livro; cidade mediterrânea ao pôr do sol, moldura floral |
| `jardins-de-inverno.png` | Mangá; personagem original, desenho em preto/branco e flores vermelhas |
| `mare-de-estrelas.png` | Filme; costa noturna, farol, céu estrelado e duas pessoas |
| `rota-do-norte.png` | Série/anime; trem vermelho, neve e montanhas em luz rosada |

Todas têm 1024×1536 pixels, proporção 2:3, e foram inspecionadas na geração e na galeria renderizada. Cada uma é um asset raster independente, sem captura da tela inteira como interface. Os hashes SHA-256 dos assets distribuídos são conferidos por `scripts/Verify-Release.ps1` e registrados no manifesto de entrega. As capas não são invertidas no tema escuro.

Fontes, ícones, textos, normalização e capas demonstrativas estão no pacote local. A consulta opcional de filmes usa a API e o CDN de imagens do TMDB. As demais telas usam os recursos locais, e notas pessoais ficam na base local.

A 0.12 limita o cache de pôsteres externos a 90 dias no diretório de dados. O cache fica fora do pacote e de backups da coleção. A janela de 90 dias é uma escolha de implementação para retenção limitada; os termos da API continuam sendo a referência do fornecedor. [Licença e atribuição do TMDB](https://developer.themoviedb.org/docs/faq)
