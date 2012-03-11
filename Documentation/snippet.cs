                   var word2 = word;
                    if (word.ContainsBastardisedTweetdeckURL())
                        word2 = "http://" + word;
                    if (word2.ContainsHyperlink())
                    {
                        MatchCollection matches = word2.GetHyperlinks();
                        int pos = 0;
                        foreach (Match m in matches)
                        {
                            try
                            {
                                if (pos < m.Index)
                                {
                                    tr.End.Paragraph.Inlines.Add(word2.Substring(pos, m.Index - pos));
                                }

                                string wordURL = m.Value;
                                if (!wordURL.StartsWith("http://", true, null) &&
                                    !wordURL.StartsWith("https://", true, null) &&
                                    !wordURL.StartsWith("ftp://", true, null))
                                {
                                    wordURL = "http://" + wordURL;
                                }

                                InlineLink il = new InlineLink();
                                il.Url = new Uri(wordURL);
                                il.Text = ShortenVisualInlineURL(wordURL) + Ellipsis;
                                il.Foreground = _linkBrush;
                                il.ToolTip = wordURL;
                                il.Tag = textblock;
                                il.MouseLeftButtonDown += LinkClick;
                                tr.End.Paragraph.Inlines.Add(il);

                                // If it contains any of the known url shortening services, expand the url in the background
                                if (applicationSettings.AutoExpandUrls && wordURL.IsShortUrl())
                                {
                                    il.Dispatcher.ExecuteAsync(
                                        () =>
                                        {
                                            try
                                            {
                                                var urlExpanders = CompositionManager.Get<IUrlExpandService>();
                                                var singleUrl = urlExpanders.ExpandUrl(wordURL);

                                                string expanded;
                                                if (singleUrl.TryGetValue(wordURL, out expanded) && expanded != null)
                                                {
                                                    il.Url = new Uri(expanded);
                                                    il.ToolTip = expanded;

                                                    il.Text = ShortenVisualInlineURL(expanded) + Ellipsis;

                                                    var image = new CachedImage
                                                                    {
                                                                        Url = new Uri("http://" + ShortenVisualInlineURL(expanded) + "/favicon.ico"),
                                                                        MaxHeight = 10,
                                                                        MaxWidth = 10,
                                                                        Margin = new Thickness(0, 0, 3, 0)
                                                                    };

                                                    il.Image = image;

                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                MahTweets.Core.Composition.CompositionManager.Get<MahTweets.Core.Interfaces.Application.IExceptionReporter>().ReportHandledException(ex);
                                            }

                                        });
                                    //  }
                                }
                            }
                            catch
                            {
                                tr.End.Paragraph.Inlines.Add(m.Value);
                            }

                            pos = m.Index + m.Length;
                        }

                        // Add the remaining text to this textblock
                        if (pos < word.Length)
                        {
                            tr.End.Paragraph.Inlines.Add(word.Substring(pos));
                        }
                    }

                        //If its a "mention" of somebody, link to their profile (for now)
                    else if (
                        (
                            word.StartsWith("@")
                            ||
                            word.StartsWith(".@") // 'cool kid' trying to bypass broken @replies
                            ||
                            word.StartsWith("-@") // ... 
                            ||
                            word.StartsWith(":@") // ...
                            ||
                            word.StartsWith("r@") // ... 
                            ||
                            word.StartsWith("cc@") // ... 
                            ||
                            word.StartsWith("rt@") // semi broken RT?
                            ||
                            word.StartsWith("(@") // enclosed in brackets
                            ||
                            word.StartsWith("\"@") // beginning of a quote
                            ||
                            word.StartsWith("“@") // beginning of a hipster smart quote
                            ||
                            word.StartsWith("\\@") // beginning of a backslash
                            ||
                            word.StartsWith("/@") // beginning of a forward slash. One day, I will regex the shit out of this
                            ||
                            word.StartsWith("+@") // beginning of a forward slash. One day, I will regex the shit out of this
                        )
                        &&
                        word != "@")
                    {
                        string tmpword = word;
                        if (!word.StartsWith("@"))
                        {
                            tr.End.Paragraph.Inlines.Add(word.Substring(0, word.IndexOf("@")));
                            tmpword = word.Substring(word.IndexOf("@"));
                        }

                        var matches = Regex.Match(tmpword, "@([_A-Za-z0-9]*)");
                        InlineLink il = new InlineLink();
                        il.Text = tmpword;
                        il.Foreground = _linkBrush;
                        il.TextDecorations = null;
                        il.ToolTip = "View user profile";
                        il.MouseLeftButtonDown += (s, e) =>
                        {
                            PluginEventHandler.FireEvent("profileByName", (IStatusUpdate)textblock.DataContext, matches.Groups[0].Value.Substring(1));
                        };
                        il.MouseEnter += (s, e) =>
                        {
                            PluginEventHandler.FireEvent("hoverByName_mouseEnter", (IStatusUpdate)textblock.DataContext, matches.Groups[0].Value.Substring(1));
                        };
                        il.MouseLeave += (s, e) =>
                        {
                            PluginEventHandler.FireEvent("hoverByName_mouseLeave", (IStatusUpdate)textblock.DataContext, matches.Groups[0].Value.Substring(1));
                        };
                        tr.End.Paragraph.Inlines.Add(il);
                    }

                    else if (word.StartsWith("#") && word != "#")
                    {
                        InlineLink il = new InlineLink();
                        il.Tag = textblock;
                        il.Text = word;
                        il.TextDecorations = null;
                        il.Foreground = _linkBrush;
                        il.MouseLeftButtonDown += (s, e) =>
                        {
                            PluginEventHandler.FireEvent("searchHashtag", (IStatusUpdate)textblock.DataContext, il.Text);
                        };
                        il.ToolTip = "Search for " + word;

                        ContextMenu cm = new ContextMenu();
                        MenuItem miIgnore = new MenuItem();
                        miIgnore.Click += (s, e) =>
                        {
                            applicationSettings.GlobalExclude.Add(word);
                        };

                        miIgnore.Header = "Globally ignore this hashtag";
                        miIgnore.Tag = textblock;

                        cm.Items.Add(miIgnore);

                        il.ContextMenu = cm;

                        tr.End.Paragraph.Inlines.Add(il);
                    } 
                    else
                    {
                        tr.End.Paragraph.Inlines.Add(word);
                    }
