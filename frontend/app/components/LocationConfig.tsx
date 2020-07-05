import React, { useState, useCallback } from 'react';
import { View, StyleSheet, Switch, Text } from 'react-native';
import { useMounted } from '../CommonJs/Hooks';
import { useApp } from '../lib/hooks';
import Log from '../CommonJs/Log';
import Busy from './Busy';
import { inputLineStyle } from '../lib/style';
import { delayAsync } from '../CommonJs/utilTs';
import Button from './Button';
import TextInput from './TextInput';
import Header from './Header';

interface LocationConfigProps
{
    add?:boolean;
    locationId?:string;
}

export default function LocationConfig({
    add,
    locationId
}:LocationConfigProps){

    const mt=useMounted();

    const {locations,history}=useApp();
    const location=locations.getLocation(locationId)

    const [name,setName]=useState(location?.Name||'');
    const [url,setUrl]=useState(location?.ApiBaseUrl||'');
    const [token,setToken]=useState(location?.Token||'');
    const [notifications,setNotifications]=useState(location?location.ReceiveNotifications:true);

    const [busy,setBusy]=useState(false);

    
    const submit=useCallback(async ()=>{

        if(busy){
            return;
        }

        if(!name || !url || !token){
            Log.error('Name, Url and Token required');
            return;
        }

        setBusy(true);
        try{
            await locations.setLocationAsync(location?location.Id:null,name,url,token,notifications);
            await delayAsync(1500);
        }catch(ex){
            Log.error('Unable to '+(add?'add':'save')+' location - '+ex.message,ex);
            return;
        }finally{
            setBusy(false);
        }
        if(!mt.mounted){return;}

        if(add){
            history.reset('/');
        }

    },[name,url,token,notifications,busy,mt,locations,history,add,location]);

    const [confirmDelete,setConfirmDelete]=useState(false);
    const deleteLocation=useCallback(async ()=>{

        if(!location){
            return;
        }

        if(await locations.deleteLocationAsync(location.Id) && mt.mounted){
            history.reset('/');
        }

    },[mt,locations,location,history]);

    return (
        <>
            <View style={[styles.root,{opacity:busy?0.3:1}]}>
                <Header pop title={location?.Name||(add?'New Location':'...')} icon="cog"/>
                <TextInput placeholder="Name" value={name} onChangeText={v=>setName(v)} />
                <TextInput placeholder="Url" value={url} onChangeText={v=>setUrl(v)} />
                {add&&<TextInput placeholder="Token" value={token} onChangeText={v=>setToken(v)} />}
                <View style={inputLineStyle}>
                    <Text>Receive Notifications</Text>
                    <Switch value={notifications} onValueChange={setNotifications} />
                </View>


                <Button primary icon="plus" title={add?'Add':'Save'} onPress={submit} />

                {!add&&<>
                    {!confirmDelete&&<Button title="Delete" onPress={()=>setConfirmDelete(true)} />}
                    {confirmDelete&&<Button title="Really Delete" onPress={deleteLocation} />}
                </>}
            </View>
            {busy&&<Busy touchable/>}
        </>
    )

}

const styles=StyleSheet.create({
    root:{
        flex:1
    }
});
