const t=`---
title: Digitally Signed App
slug: 2023-04-29-signpath
description: Race Element is now released with a certificate with SignPath
date: 2023-04-29 
type: news
---

<h1 id="certificates-and-trusted-sources">Certificates and trusted sources</h1>
<p>Some time ago I have gotten a bit frustrated that the app was not signed with a certificate. At least a certificate that browsers and other security software use to check for legitimate software.
Browsers would keep warning users that the app was probably not secure.
With the app being build and the source being verified each build will be trusted by at least browser.</p>
<h1 id="signpath-foundation">SignPath Foundation</h1>
<p>SignPath Foundation provides reliable code signing for Open Source projects.
No more installation warnings. Certificates provided by SignPath Foundation are recognized by operating systems, browsers and Java.
No more untrusted binaries. Rest assured that signed code is compiled straight from the source code repository. If you trust the source, you can trust the binary.
<a href="https://www.signpath.io">
    <img src="https://about.signpath.io/assets/signpath-logo.svg" width="150">
</a>
Free code signing provided by <a href="https://signpath.io?utm_source=foundation&utm_medium=github&utm_campaign=race-element">SignPath.io</a>, certificate by <a href="https://signpath.org?utm_source=foundation&utm_medium=github&utm_campaign=race-element">SignPath Foundation</a></p>
`;export{t as default};
