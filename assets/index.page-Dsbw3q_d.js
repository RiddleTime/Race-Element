import{ɵ as u,o as l,a as p,g as a,h as i,j as s,p as x,l as d,q as m,R as _,C as c,n as b,r as g,m as f,s as v}from"./index-Chmq-TDm.js";const C=(t,n)=>n.attributes.slug,h=t=>["/guide/",t];function I(t,n){if(t&1&&(a(0,"a",3)(1,"div",4)(2,"div",5)(3,"h2",6),i(4),s()(),a(5,"div",7)(6,"p",8),i(7),s()()()()),t&2){const r=n.$implicit;b("routerLink",g(3,h,r.attributes.slug)),d(4),f("",r.attributes.title," "),d(3),v(r.attributes.description)}}let G=(()=>{var t;class n{constructor(){this.posts=l(e=>e.attributes.type==="guide")}ngOnInit(){this.posts.sort((e,o)=>o.attributes.title<e.attributes.title?1:o.attributes.title>e.attributes.title?-1:0)}}return t=n,t.ɵfac=function(e){return new(e||t)},t.ɵcmp=u({type:t,selectors:[["app-guides"]],standalone:!0,features:[p],decls:6,vars:0,consts:[[1,"mx-auto","rounded-lg","shadow-lg","select-none","container","max-w-4xl","px-2"],[1,"font-['Conthrax']","text-4xl","mb-2","md:mb-1","text-center"],[1,"container","mx-auto","flex-wrap"],[3,"routerLink"],[1,"container","bg-[rgba(0,0,0,0.7)]","mb-3","hover:bg-[#191919]","hover:border-[transparent]","hover:border-l-2","rounded-br-lg","rounded-tl-xl","max-w-4xl","mx-auto","text-pretty"],[1,"container","text-gray-300","bg-[#030303]","rounded-tl-xl","pl-2","pr-2","pt-1","pb-1","border-l-2","border-[red]"],[1,"font-['Conthrax']","text-xl","md:text-3xl","pl-1","text-white"],[1,"container","ml-3","pb-1"],[1,"text-sm","md:text-lg","ml-1","mr-1","text-[rgba(255,255,255,0.78)]"]],template:function(e,o){e&1&&(a(0,"div",0)(1,"h1",1),i(2,"Guides"),s(),a(3,"div",2),x(4,I,8,5,"a",3,C),s()()),e&2&&(d(4),m(o.posts))},dependencies:[_,c]}),n})();export{G as default};
