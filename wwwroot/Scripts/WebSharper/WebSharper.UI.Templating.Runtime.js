(function(Global)
{
 "use strict";
 var WebSharper,UI,Templating,Runtime,Server,TemplateInitializer,Obj,TemplateInstances,Handler,ProviderBuilder,TemplateInstance,Client,ClientTemplateInstanceHandlers,Collections,Dictionary,Runtime$1,Arrays,Unchecked,Var$1,Operators,Doc,Client$1,Templates,Slice,BindVar,console,View,Activator,HashSet,Enumerator,Seq,System,Guid;
 WebSharper=Global.WebSharper=Global.WebSharper||{};
 UI=WebSharper.UI=WebSharper.UI||{};
 Templating=UI.Templating=UI.Templating||{};
 Runtime=Templating.Runtime=Templating.Runtime||{};
 Server=Runtime.Server=Runtime.Server||{};
 TemplateInitializer=Server.TemplateInitializer=Server.TemplateInitializer||{};
 Obj=WebSharper&&WebSharper.Obj;
 TemplateInstances=Server.TemplateInstances=Server.TemplateInstances||{};
 Handler=Server.Handler=Server.Handler||{};
 ProviderBuilder=Server.ProviderBuilder=Server.ProviderBuilder||{};
 TemplateInstance=Server.TemplateInstance=Server.TemplateInstance||{};
 Client=Runtime.Client=Runtime.Client||{};
 ClientTemplateInstanceHandlers=Client.ClientTemplateInstanceHandlers=Client.ClientTemplateInstanceHandlers||{};
 Collections=WebSharper&&WebSharper.Collections;
 Dictionary=Collections&&Collections.Dictionary;
 Runtime$1=WebSharper&&WebSharper.Runtime;
 Arrays=WebSharper&&WebSharper.Arrays;
 Unchecked=WebSharper&&WebSharper.Unchecked;
 Var$1=UI&&UI.Var$1;
 Operators=WebSharper&&WebSharper.Operators;
 Doc=UI&&UI.Doc;
 Client$1=UI&&UI.Client;
 Templates=Client$1&&Client$1.Templates;
 Slice=WebSharper&&WebSharper.Slice;
 BindVar=UI&&UI.BindVar;
 console=Global.console;
 View=UI&&UI.View;
 Activator=WebSharper&&WebSharper.Activator;
 HashSet=Collections&&Collections.HashSet;
 Enumerator=WebSharper&&WebSharper.Enumerator;
 Seq=WebSharper&&WebSharper.Seq;
 System=Global.System;
 Guid=System&&System.Guid;
 TemplateInitializer.$cctor=function()
 {
  TemplateInitializer.$cctor=Global.ignore;
  TemplateInitializer.initialized=new Dictionary.New$5();
 };
 TemplateInitializer=Server.TemplateInitializer=Runtime$1.Class({
  get_Vars:function()
  {
   TemplateInitializer.$cctor();
   return this.vars;
  },
  get_Id:function()
  {
   TemplateInitializer.$cctor();
   return this.id;
  },
  InitInstance:function(key)
  {
   var $1,d,a,i,$2;
   TemplateInitializer.$cctor();
   d=TemplateInitializer.GetHolesFor(key);
   a=this.vars;
   for(i=0,$2=a.length-1;i<=$2;i++)(function()
   {
    var f,t,ov,n,o,o$1,o$2;
    f=Arrays.get(a,i);
    t=f[1];
    ov=f[2];
    n=f[0];
    return!d.ContainsKey(n)?d.set_Item(n,Unchecked.Equals(t,0)?{
     $:10,
     $0:n,
     $1:Var$1.Create$1((o=ov==null?null:{
      $:1,
      $0:ov.$0
     },o==null?"":o.$0))
    }:Unchecked.Equals(t,1)?{
     $:15,
     $0:n,
     $1:Var$1.Create$1((o$1=ov==null?null:{
      $:1,
      $0:ov.$0
     },o$1==null?0:o$1.$0))
    }:Unchecked.Equals(t,2)?{
     $:11,
     $0:n,
     $1:Var$1.Create$1((o$2=ov==null?null:{
      $:1,
      $0:ov.$0
     },o$2==null?false:o$2.$0))
    }:Operators.FailWith("Invalid value type")):null;
   }());
   this.instance=new TemplateInstance.New({
    $:0,
    $0:d
   },Doc.get_Empty());
  },
  get_Instance:function()
  {
   TemplateInitializer.$cctor();
   return Runtime$1.GetOptional(this.instance).$0;
  },
  $postinit:function(key)
  {
   TemplateInitializer.$cctor();
   Templates.RunFullDocTemplate([]);
  },
  $init:function(a)
  {
   TemplateInitializer.$cctor();
  },
  $preinit:function(key)
  {
   var q,i,$1,el,fullName,s,hole,hole$1;
   TemplateInitializer.$cctor();
   this.InitInstance(key);
   q=self.document.querySelectorAll("[ws-var^='"+key+"::']");
   for(i=0,$1=q.length-1;i<=$1;i++){
    el=q[i];
    fullName=el.getAttribute("ws-var");
    s=Slice.string(fullName,{
     $:1,
     $0:key.length+2
    },null);
    hole=this.get_Instance().Hole(s);
    hole$1={
     $:hole.$,
     $0:fullName,
     $1:hole.$1
    };
    Templates.GlobalHoles().set_Item(hole$1.$0,hole$1);
    TemplateInitializer.applyVarHole(el,hole);
   }
  }
 },Obj,TemplateInitializer);
 TemplateInitializer.applyVarHole=function(el,tpl)
 {
  var $1;
  TemplateInitializer.$cctor();
  switch(tpl.$==11?1:tpl.$==12?2:tpl.$==13?3:tpl.$==14?4:tpl.$==15?5:tpl.$==0?($1=tpl.$0,6):tpl.$==1?($1=tpl.$0,6):tpl.$==2?($1=tpl.$0,6):tpl.$==16?($1=tpl.$0,6):tpl.$==4?($1=tpl.$0,6):tpl.$==5?($1=tpl.$0,6):tpl.$==6?($1=tpl.$0,6):tpl.$==7?($1=tpl.$0,6):tpl.$==8?($1=tpl.$0,6):tpl.$==9?($1=tpl.$0,6):tpl.$==3?($1=tpl.$0,6):0)
  {
   case 0:
    TemplateInitializer.applyTypedVarHole(BindVar.StringApply(),tpl.$1,el);
    break;
   case 1:
    TemplateInitializer.applyTypedVarHole(BindVar.BoolCheckedApply(),tpl.$1,el);
    break;
   case 2:
    TemplateInitializer.applyTypedVarHole(BindVar.IntApplyChecked(),tpl.$1,el);
    break;
   case 3:
    TemplateInitializer.applyTypedVarHole(BindVar.IntApplyUnchecked(),tpl.$1,el);
    break;
   case 4:
    TemplateInitializer.applyTypedVarHole(BindVar.FloatApplyChecked(),tpl.$1,el);
    break;
   case 5:
    TemplateInitializer.applyTypedVarHole(BindVar.FloatApplyUnchecked(),tpl.$1,el);
    break;
   case 6:
    console.warn("Not a var hole: ",$1);
    break;
  }
 };
 TemplateInitializer.applyTypedVarHole=function(bind,v,el)
 {
  var p;
  TemplateInitializer.$cctor();
  p=bind(v);
  p[0](el);
  View.Sink(p[1](el),p[2]);
 };
 TemplateInitializer.GetOrAddHoleFor=function(id,holeName,initHole)
 {
  var d,m,o,h;
  TemplateInitializer.$cctor();
  d=TemplateInitializer.GetHolesFor(id);
  m=(o=null,[d.TryGetValue(holeName,{
   get:function()
   {
    return o;
   },
   set:function(v)
   {
    o=v;
   }
  }),o]);
  return m[0]?m[1]:(h=initHole(),(d.set_Item(holeName,h),h));
 };
 TemplateInitializer.GetHolesFor=function(id)
 {
  var m,o,d;
  TemplateInitializer.$cctor();
  m=(o=null,[TemplateInitializer.initialized.TryGetValue(id,{
   get:function()
   {
    return o;
   },
   set:function(v)
   {
    o=v;
   }
  }),o]);
  return m[0]?m[1]:(d=new Dictionary.New$5(),(TemplateInitializer.initialized.set_Item(id,d),d));
 };
 TemplateInitializer.get_Initialized=function()
 {
  TemplateInitializer.$cctor();
  return TemplateInitializer.initialized;
 };
 TemplateInitializer.New=Runtime$1.Ctor(function(id,vars)
 {
  TemplateInitializer.$cctor();
  Obj.New.call(this);
  this.vars=vars;
  Runtime$1.SetOptional(this,"instance",null);
  this.id=id;
 },TemplateInitializer);
 TemplateInstances=Server.TemplateInstances=Runtime$1.Class({},Obj,TemplateInstances);
 TemplateInstances.GetInstance=function(key)
 {
  return(Activator.Instances())[key].get_Instance();
 };
 TemplateInstances.New=Runtime$1.Ctor(function()
 {
  Obj.New.call(this);
 },TemplateInstances);
 Handler.AfterRenderQ2$213$42=function(key,f)
 {
  return function(el)
  {
   f({
    Vars:TemplateInstances.GetInstance(key),
    Target:el,
    Event:null
   });
  };
 };
 Handler.EventQ2$199$36=function(key,f)
 {
  return function(el)
  {
   return function(ev)
   {
    return f({
     Vars:TemplateInstances.GetInstance(key),
     Target:el,
     Event:ev
    });
   };
  };
 };
 Handler.CompleteHoles=function(key,filledHoles,vars)
 {
  var allVars,filledVars,e,h,n;
  function c(name,ty,a)
  {
   var r;
   return filledVars.Contains(name)?null:(r=Unchecked.Equals(ty,0)?TemplateInitializer.GetOrAddHoleFor(key,name,function()
   {
    return{
     $:10,
     $0:name,
     $1:Var$1.Create$1("")
    };
   }):Unchecked.Equals(ty,1)?TemplateInitializer.GetOrAddHoleFor(key,name,function()
   {
    return{
     $:15,
     $0:name,
     $1:Var$1.Create$1(0)
    };
   }):Unchecked.Equals(ty,2)?TemplateInitializer.GetOrAddHoleFor(key,name,function()
   {
    return{
     $:11,
     $0:name,
     $1:Var$1.Create$1(false)
    };
   }):Operators.FailWith("Invalid value type"),(allVars.set_Item(name,r),{
    $:1,
    $0:r
   }));
  }
  allVars=new Dictionary.New$5();
  filledVars=new HashSet.New$3();
  e=Enumerator.Get(filledHoles);
  try
  {
   while(e.MoveNext())
    {
     h=e.Current();
     n=h.$0;
     filledVars.SAdd(n);
     allVars.set_Item(n,h);
    }
  }
  finally
  {
   if(typeof e=="object"&&"Dispose"in e)
    e.Dispose();
  }
  return[Seq.append(filledHoles,Arrays.choose(function($1)
  {
   return c($1[0],$1[1],$1[2]);
  },vars)),{
   $:0,
   $0:allVars
  }];
 };
 Handler.AfterRenderQ2=function(key,holeName,ti,f)
 {
  return{
   $:8,
   $0:holeName,
   $1:function(el)
   {
    f({
     Vars:ti(),
     Target:el,
     Event:null
    });
   }
  };
 };
 Handler.EventQ2=function(key,holeName,ti,f)
 {
  return{
   $:5,
   $0:holeName,
   $1:function(el)
   {
    return function(ev)
    {
     return f({
      Vars:ti(),
      Target:el,
      Event:ev
     });
    };
   }
  };
 };
 ProviderBuilder=Server.ProviderBuilder=Runtime$1.Class({},Obj,ProviderBuilder);
 ProviderBuilder.New=Runtime$1.Ctor(function(src)
 {
  var c;
  Obj.New.call(this);
  this.i=null;
  this.k=(c=Guid.NewGuid(),Global.String(c));
  this.h=Runtime$1.MarkResizable([]);
  this.s=src;
 },ProviderBuilder);
 ProviderBuilder.New$1=Runtime$1.Ctor(function()
 {
  var c;
  Obj.New.call(this);
  this.i=null;
  this.k=(c=Guid.NewGuid(),Global.String(c));
  this.h=Runtime$1.MarkResizable([]);
  Runtime$1.SetOptional(this,"s",null);
 },ProviderBuilder);
 TemplateInstance=Server.TemplateInstance=Runtime$1.Class({
  Hole:function(name)
  {
   return this.allVars.Item(name);
  },
  get_Doc:function()
  {
   return this.doc;
  }
 },Obj,TemplateInstance);
 TemplateInstance.New=Runtime$1.Ctor(function(c,doc)
 {
  Obj.New.call(this);
  this.doc=doc;
  this.allVars=c.$==0?c.$0:Operators.FailWith("Should not happen");
 },TemplateInstance);
 ClientTemplateInstanceHandlers.AfterRenderQ2Client=function(key,el,f)
 {
  f({
   Vars:TemplateInstances.GetInstance(key),
   Target:el,
   Event:null
  });
 };
 ClientTemplateInstanceHandlers.EventQ2Client=function(key,el,ev,f)
 {
  f({
   Vars:TemplateInstances.GetInstance(key),
   Target:el,
   Event:ev
  });
 };
 Client.AfterRenderQ2$57$26=function(f)
 {
  return function()
  {
   f();
  };
 };
 Client.Box=Global.id;
}(self));
