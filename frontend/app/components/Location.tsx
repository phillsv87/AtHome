import React, { useEffect } from 'react';
import { useApp } from '../lib/hooks';
import { useAsync } from '../CommonJs/AsyncHooks';
import Log from '../CommonJs/Log';
import { StreamInfo } from '../lib/types';
import { View } from 'react-native';
import Busy from './Busy';
import { useEmitter } from '../CommonJs/EventEmitterEx-rn';
import { StateChangeEvent } from '../lib/NativeDevice';
import Button from './Button';
import { useScrollable } from '../CommonJs/ScrollContext';
import Header from './Header';

interface LocationProps
{
    locationId?:string;
}

export default function Location({
    locationId
}:LocationProps){

    const {http,config,history,locations,device}=useApp();

    const {devicePushId}=device.getState();
    useEmitter(device,StateChangeEvent);

    useScrollable();

    const location=locations.getLocation(locationId);

    const streams=useAsync(null,async ()=>{

        if(!location){
            return null;
        }

        try{
            return await http.getAsync<StreamInfo[]>(`${location.ApiBaseUrl}api/Stream`,{clientToken:location.Token});
        }catch(ex){
            Log.error('Unable to load stream list',ex);
        }

    },[http,config,location]);

    useEffect(()=>{

        if(!location || !location.ReceiveNotifications){
            return;
        }

        if(!devicePushId){
            device.requestNotificationsPermission();
            return;
        }

        const run=async ()=>{
            try{
                const r=await locations.updateNotificationsSettings(location.Id);
                if(r){
                    Log.infoUI('Notification settings updated');
                }
            }catch(ex){
                Log.error('Unable to update Notification settings for location',ex);
            }
        }
        run();

    },[location,locations,devicePushId,device]);

    if(!streams || !location){
        return <Busy/>
    }

    return (
        <View>
            <Header pop title={location.Name} icon="home"/>
            {streams.map((s)=>(
                <Button title={s.Name} key={s.Id} onPress={()=>history.push(`/location/${location.Id}/stream/${s.Id}`)} />
            ))}
            <Button icon="cog" primary title="Config" onPress={()=>history.push(`/location/${location.Id}/config`)}/>
        </View>
    )

}
