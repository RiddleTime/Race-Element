import{ɵ as o,a as s,v as r,b as u,h as c,j as p,k as l}from"./index-CIFYlOo7.js";const m={meta:[{property:"og:title",content:"Race Element - SetupLink"},{property:"twitter:title",content:"Race Element - SetupLink"}]};let _=(()=>{var t;class a{constructor(e){this.route=e,this.setupLink=""}ngOnInit(){this.route.queryParams.subscribe(e=>{let n=e.link;n!==void 0&&(this.setupLink=n,console.log(this.setupLink),window.location.assign("RaceElement://setup="+this.setupLink))})}}return t=a,t.ɵfac=function(e){return new(e||t)(o(r))},t.ɵcmp=s({type:t,selectors:[["ng-component"]],standalone:!0,features:[u],decls:3,vars:0,consts:[[1,"container","mx-auto","text-center","select-none"]],template:function(e,n){e&1&&(c(0,"div",0)(1,"h2"),p(2,"Trying to Open Race Element's Setup Importer"),l()())},encapsulation:2}),a})();export{_ as default,m as routeMeta};