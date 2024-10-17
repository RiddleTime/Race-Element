const e=`---
title: New website
slug: 2024-06-12-new-website-2024
description: From Ruby to Angular
date: 2024-06-12
type: news
---

<h1 id="old-website-with-ruby-impossible-to-update">Old website with Ruby impossible to update</h1>
<p>A week ago I noticed the build was failing for the ruby website. It was a really nifty system where I only had to write articles in markdown format. Since the SetupLinks were added I wanted to add a few additional changes but after a bit of research I noticed that the system in use was end of life. 
Where I already manually started editing the build result, it would defeat the purpose of writing articles in markdown. It was a chapter that came to and end but sometimes these things happen and we need to move on.</p>
<h1 id="new-website-now-uses-angular">New website now uses Angular</h1>
<p>As I was familiar with Angular and even AngularJS, the choice was fairly easy. The only issue was that I didn&#39;t want to rewrite all articles and guides from scratch. So the choice for static site generation was mandatory.
The website is now generated with <a href="https://analogjs.org/">Analog</a> which allows me to continue writing articles with markdown whilst splitting up the news and guide section. Additionally the website is now entirely designed in-house. So we have full control!</p>
<h1 id="changes">Changes</h1>
<p>SetupLinks have been changed, instead of <code>https://race.elementfuture.com?setup=</code> the base has now become <code>https://race.elementfuture.com/setup?link=</code> 
The app will be updated to handle this change. Unfortunately you will have to do this change manually for SetupLinks that you have already created or have received. 
The actual data system has not changed so by just adjusting the first part of the link your SetupLink will open Race Element again.</p>
`;export{e as default};
