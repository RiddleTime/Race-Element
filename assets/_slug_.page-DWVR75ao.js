import{i as m,ɵ as u,M as _,a as g,b as f,c as b,d as c,e as h,f as w,A as x,g as C,C as y,D as k,j as l,k as s,l as n,m as v,n as o,o as D,u as N,v as I,p as M}from"./index-DJruGiuI.js";function T(i,e){if(i&1&&(l(0,"article",0)(1,"h1",1)(2,"a",2),s(3,"News"),n(),s(4),n(),l(5,"div",3)(6,"p",4),s(7),c(8,"date"),n(),v(9,"analog-markdown",5),n()()),i&2){const t=e;o(4),D(" > ",t.attributes.title," "),o(3),N(t.attributes.date!==void 0?I(8,3,t.attributes.date,"longDate"):""),o(2),M("content",t.content)}}const a=class a{constructor(e){this.meta=e,this.post=m({param:"slug",subdirectory:"news"})}ngOnInit(){this.post.forEach(e=>{this.meta.updateTag({name:"og:url",content:`https://race.elementfuture.com/news/${e.attributes.slug}`}),this.meta.updateTag({name:"og:title",content:`Race Element - News | ${e.attributes.title}`}),this.meta.updateTag({name:"og:description",content:`${e.attributes.description}`}),this.meta.updateTag({name:"twitter:title",content:`Race Element - News | ${e.attributes.title}`})})}};a.ɵfac=function(t){return new(t||a)(u(_))},a.ɵcmp=g({type:a,selectors:[["app-news-post"]],standalone:!0,features:[f],decls:2,vars:3,consts:[[1,"rounded-lg","container","mx-auto","max-w-4xl","px-3"],[1,"text-xl","md:text-3xl","font-['Conthrax']","select-none","dark:text-gray-300","dark:bg-black","rounded-tl-xl","border-l-2","pl-2","pr-2","pt-1","pb-1","border-red-800"],["href","/news"],[1,"container","dark:bg-[#050505]","pl-3","pr-[1em]","pt-2","rounded-br-xl"],[1,"select-none","text-sm","mb-3"],[1,"whitespace-pre-line",3,"content"]],template:function(t,d){if(t&1&&(b(0,T,10,6,"article",0),c(1,"async")),t&2){let r;h((r=w(1,1,d.post))?0:-1,r)}},dependencies:[x,C,y,k],encapsulation:2});let p=a;export{p as default};
