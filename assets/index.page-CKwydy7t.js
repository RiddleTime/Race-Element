import{q as u,a as c,b as x,j as r,k as o,l as s,r as m,n as i,s as _,R as b,C as f,D as g,d as h,p as w,t as v,o as C,u as p,v as y}from"./index-DJruGiuI.js";const D=(a,e)=>e.attributes.slug,I=a=>["/news/",a];function N(a,e){if(a&1&&(r(0,"a",3)(1,"div",4)(2,"div",5)(3,"h2",6),o(4),s(),r(5,"p",7),o(6),h(7,"date"),s()(),r(8,"div",8)(9,"p",9),o(10),s()()()()),a&2){const t=e.$implicit;w("routerLink",v(7,I,t.attributes.slug)),i(4),C("",t.attributes.title," "),i(2),p(y(7,4,t.attributes.date,"longDate")),i(4),p(t.attributes.description)}}const M={meta:[{property:"og:title",content:"Race Element - News"},{property:"twitter:title",content:"Race Element - News"}]},n=class n{constructor(){this.posts=u(e=>e.attributes.type==="news")}ngOnInit(){this.posts.sort((e,t)=>e.attributes.date===void 0||t.attributes.date===void 0?e.attributes.date!==void 0?-1:t.attributes.title<e.attributes.title?1:t.attributes.title>e.attributes.title?-1:0:e.attributes.date<t.attributes.date?1:-1)}};n.ɵfac=function(t){return new(t||n)},n.ɵcmp=c({type:n,selectors:[["app-news"]],standalone:!0,features:[x],decls:6,vars:0,consts:[[1,"mx-auto","rounded-lg","shadow-lg","select-none","container","max-w-4xl","px-3"],[1,"font-['Conthrax']","text-4xl","mb-1","text-center"],[1,"container","mx-auto","flex-wrap"],[3,"routerLink"],[1,"container","bg-[rgba(0,0,0,0.7)]","mb-3","hover:bg-[#191919]","hover:border-[transparent]","hover:border-l-2","rounded-br-lg","rounded-tl-xl","mx-auto","text-pretty"],[1,"container","text-gray-300","bg-[#030303]","rounded-tl-xl","pl-2","pr-2","pt-1","pb-1","border-l-2","border-[red]"],[1,"font-['Conthrax']","text-xl","md:text-2xl","pl-1","text-white"],[1,"text-xs","ml-1","mt-1","text-[rgba(255,70,0,0.8)]","mx-auto"],[1,"container","ml-3","pr-[1em]","pb-1","text-pretty"],[1,"text-sm","md:text-base","ml-1","mr-1","text-[rgba(255,255,255,0.78)]"]],template:function(t,l){t&1&&(r(0,"div",0)(1,"h1",1),o(2,"News"),s(),r(3,"div",2),m(4,N,11,9,"a",3,D),s()()),t&2&&(i(4),_(l.posts))},dependencies:[b,f,g]});let d=n;export{d as default,M as routeMeta};
